using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Security;

internal static class EntraIdExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds a Bicep-provisioned Microsoft Graph deployment for Entra Agent Identity
        ///     registrations (blueprint, Chat agent, and Rating agent) when publishing to Azure.
        /// </summary>
        /// <param name="name">The name of the Bicep template resource.</param>
        /// <param name="entraId">The parent Entra ID application resource builder.</param>
        /// <returns>
        ///     An <see cref="IResourceBuilder{AzureBicepResource}" /> when in publish mode; otherwise, <see langword="null" />.
        /// </returns>
        public IResourceBuilder<AzureBicepResource>? AddEntraAgentIdentities(
            string name,
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            if (!builder.ExecutionContext.IsPublishMode)
            {
                return null;
            }

            return builder
                .AddBicepTemplate(name, "Infra/entra-agent-identities.bicep")
                .WithParameter("tenantId", entraId.Resource.TenantIdOutputReference)
                .WithParameter("parentServicePrincipalId", entraId.GetOutput("servicePrincipalId"))
                .WithParameter(
                    "uniqueSuffix",
                    $"{builder.Environment.EnvironmentName.ToLowerInvariant()}-{name}"
                );
        }

        /// <summary>
        ///     Adds a Microsoft Entra ID application registration resource to the distributed application builder
        ///     with application roles, default application ID URI, ACL-based authorization, and local/publish credentials.
        /// </summary>
        /// <param name="name">The name of the Entra ID resource.</param>
        /// <returns>
        ///     An <see cref="IResourceBuilder{EntraIdApplicationResource}" /> representing the configured Entra ID resource.
        /// </returns>
        public IResourceBuilder<EntraIdApplicationResource> AddEntraId(string name)
        {
            return builder
                .AddEntraIdApplication(name)
                .WithApplicationIdUri()
                .WithAppRole(
                    new(
                        Authorization.Roles.Admin,
                        "Admin",
                        "Administrator role for managing all aspects of the application",
                        ["User", "Application"]
                    )
                )
                .WithAppRole(
                    new(
                        Authorization.Roles.BusinessUser,
                        "BusinessUser",
                        "Business user role for managing books and catalog reference data",
                        ["User", "Application"]
                    )
                )
                .WithAppRole(
                    new(
                        Authorization.Roles.Customer,
                        "Customer",
                        "Customer role for browsing and purchasing books",
                        ["User", "Application"]
                    )
                )
                .WithAllowWebApiToBeAuthorizedByACL()
                .RunWithLocalDeveloperCertificateCredential()
                .PublishWithFederatedManagedIdentityCredential();
        }
    }

    extension<TResource>(IResourceBuilder<TResource> builder)
        where TResource : IDotnetProgramResource, IResourceWithEnvironment
    {
        /// <summary>
        ///     Exposes read/write scopes for the .NET resource on the Microsoft Entra ID application registration
        ///     and injects its <c>AzureAd</c> configuration.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <returns>The resource builder for method chaining.</returns>
        public IResourceBuilder<TResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            var clientId = builder.Resource.Name;
            var clientName = Services.ToClientName(clientId, "API");

            return builder.WithReference(
                entraId
                    .WithScope(
                        $"{clientId}_{Authorization.Actions.Read}",
                        $"{nameof(Authorization.Actions.Read)} for {clientName}"
                    )
                    .WithScope(
                        $"{clientId}_{Authorization.Actions.Write}",
                        $"{nameof(Authorization.Actions.Write)} for {clientName}"
                    )
            );
        }

        /// <summary>
        ///     Injects <c>AgentIdentities</c> configuration outputs from the Bicep-provisioned
        ///     Entra Agent Identities resource into the .NET resource.
        /// </summary>
        /// <param name="agentIdentities">The Bicep agent identities resource builder, or <see langword="null" /> in run mode.</param>
        /// <param name="defaultAgentOutputName">The Bicep output name for <c>AgentIdentities:DefaultAgentId</c>.</param>
        /// <param name="agentOverrides">Optional per-service agent ID overrides mapped to Bicep output names.</param>
        /// <returns>The resource builder for method chaining.</returns>
        public IResourceBuilder<TResource> WithEntraAgentIdentity(
            IResourceBuilder<AzureBicepResource>? agentIdentities,
            string defaultAgentOutputName,
            params (string ServiceKey, string OutputName)[] agentOverrides
        )
        {
            if (agentIdentities is null)
            {
                return builder;
            }

            builder.WithEnvironment(
                "AgentIdentities__DefaultAgentId",
                agentIdentities.GetOutput(defaultAgentOutputName)
            );

            foreach (var (serviceKey, outputName) in agentOverrides)
            {
                builder.WithEnvironment(
                    $"AgentIdentities__Agents__{serviceKey}",
                    agentIdentities.GetOutput(outputName)
                );
            }

            return builder;
        }
    }

    extension(IResourceBuilder<ContainerResource> builder)
    {
        /// <summary>
        ///     Configures the container resource to integrate with Microsoft Entra ID as an Identity Provider (IdP)
        ///     via the YARP gateway.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <param name="gateway">The YARP gateway resource builder.</param>
        /// <param name="pathPrefix">Optional path prefix where the frontend is mounted on the gateway.</param>
        /// <returns>The container resource builder for method chaining.</returns>
        public IResourceBuilder<ContainerResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId,
            IResourceBuilder<YarpResource> gateway,
            string? pathPrefix = null
        )
        {
            return ConfigureFrontendEntraId(builder, entraId, gateway, pathPrefix);
        }
    }

    extension(IResourceBuilder<TurborepoAppResource> builder)
    {
        /// <summary>
        ///     Configures the Turborepo app resource to integrate with Microsoft Entra ID as an Identity Provider (IdP)
        ///     via the YARP gateway.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <param name="gateway">The YARP gateway resource builder.</param>
        /// <param name="pathPrefix">Optional path prefix where the frontend is mounted on the gateway.</param>
        /// <returns>The Turborepo app resource builder for method chaining.</returns>
        public IResourceBuilder<TurborepoAppResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId,
            IResourceBuilder<YarpResource> gateway,
            string? pathPrefix = null
        )
        {
            return ConfigureFrontendEntraId(builder, entraId, gateway, pathPrefix);
        }
    }

    private static IResourceBuilder<TResource> ConfigureFrontendEntraId<TResource>(
        IResourceBuilder<TResource> builder,
        IResourceBuilder<EntraIdApplicationResource> entraId,
        IResourceBuilder<YarpResource> gateway,
        string? pathPrefix
    )
        where TResource : IResourceWithEnvironment, IResourceWithEndpoints
    {
        var betterAuthSecret = builder
            .ApplicationBuilder.AddParameter($"{builder.Resource.Name}-better-auth-secret", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        var callbackPath = string.IsNullOrEmpty(pathPrefix)
            ? "/api/auth/oauth2/callback/microsoft-entra-id"
            : $"{pathPrefix.TrimEnd('/')}/api/auth/oauth2/callback/microsoft-entra-id";

        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            entraId.WithRedirectUris(gateway.GetEndpoint(Uri.UriSchemeHttp), callbackPath);
        }

        entraId.WithRedirectUris(gateway.GetEndpoint(Uri.UriSchemeHttps), callbackPath);

        return builder
            .WithReference(entraId)
            .WithEnvironment("BETTER_AUTH_SECRET", betterAuthSecret);
    }
}
