namespace BookWorm.AppHost.Extensions.Security;

internal static class EntraIdExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds a Microsoft Entra ID application registration resource to the distributed application builder
        ///     with application roles, ID token issuance, default application ID URI, and local/publish credentials.
        /// </summary>
        /// <param name="name">The name of the Entra ID resource.</param>
        /// <returns>
        ///     An <see cref="IResourceBuilder{EntraIdApplicationResource}" /> representing the configured Entra ID resource.
        /// </returns>
        public IResourceBuilder<EntraIdApplicationResource> AddEntraId(string name)
        {
            return builder
                .AddEntraIdApplication(name)
                .WithIdTokenIssuance()
                .WithApplicationIdUri()
                .WithAppRole(
                    new(
                        Authorization.Roles.Admin,
                        "Admin",
                        "Administrator role",
                        ["User", "Application"]
                    )
                )
                .WithAppRole(
                    new(
                        Authorization.Roles.User,
                        "User",
                        "Standard user role",
                        ["User", "Application"]
                    )
                )
                .WithAppRole(
                    new(
                        Authorization.Roles.Reporter,
                        "Reporter",
                        "Reporter role",
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
    }

    extension(IResourceBuilder<ContainerResource> builder)
    {
        /// <summary>
        ///     Configures the container resource to integrate with Microsoft Entra ID as an Identity Provider (IdP).
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <returns>The container resource builder for method chaining.</returns>
        public IResourceBuilder<ContainerResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            return ConfigureFrontendEntraId(builder, entraId);
        }
    }

    extension(IResourceBuilder<TurborepoAppResource> builder)
    {
        /// <summary>
        ///     Configures the turborepo app resource to integrate with Microsoft Entra ID as an Identity Provider (IdP).
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <returns>The turborepo app resource builder for method chaining.</returns>
        public IResourceBuilder<TurborepoAppResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            return ConfigureFrontendEntraId(builder, entraId);
        }
    }

    private static IResourceBuilder<TResource> ConfigureFrontendEntraId<TResource>(
        IResourceBuilder<TResource> builder,
        IResourceBuilder<EntraIdApplicationResource> entraId
    )
        where TResource : IResourceWithEnvironment, IResourceWithEndpoints
    {
        var betterAuthSecret = builder
            .ApplicationBuilder.AddParameter($"{builder.Resource.Name}-better-auth-secret", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        entraId.WithRedirectUris(
            builder.GetEndpoint(Uri.UriSchemeHttp),
            "/api/auth/oauth2/callback/microsoft-entra-id"
        );

        return builder
            .WithReference(entraId)
            .WithEnvironment("BETTER_AUTH_SECRET", betterAuthSecret);
    }
}
