using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Security;

internal static partial class EntraIdExtensions
{
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

        entraId.WithRedirectUris(gateway.GetBrowserEndpoint(), callbackPath);

        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            entraId.WithRedirectUris(gateway.GetEndpoint(Uri.UriSchemeHttp), callbackPath);
        }

        return builder
            .WithReference(entraId)
            .WithEnvironment("BETTER_AUTH_SECRET", betterAuthSecret);
    }
}
