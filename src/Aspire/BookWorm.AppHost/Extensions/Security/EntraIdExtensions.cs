namespace BookWorm.AppHost.Extensions.Security;

internal static partial class EntraIdExtensions
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
            [ResourceName] string name,
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
}
