using Aspire.Hosting.Yarp;
using BookWorm.AppHost.Extensions.Proxy;

namespace BookWorm.AppHost.Extensions.Security;

internal static partial class EntraIdExtensions
{
    extension<TResource>(IResourceBuilder<TResource> builder)
        where TResource : IResourceWithEnvironment, IResourceWithEndpoints
    {
        /// <summary>
        ///     Configures the frontend resource to integrate with Microsoft Entra ID as an Identity Provider (IdP)
        ///     via the YARP gateway.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <param name="gateway">The YARP gateway resource builder.</param>
        /// <param name="pathPrefix">Optional path prefix where the frontend is mounted on the gateway.</param>
        /// <returns>The frontend resource builder for method chaining.</returns>
        public IResourceBuilder<TResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId,
            IResourceBuilder<YarpResource> gateway,
            string? pathPrefix = null
        )
        {
            var betterAuthSecret = builder
                .ApplicationBuilder.AddParameter(
                    $"{builder.Resource.Name}-better-auth-secret",
                    true
                )
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
}
