using Aspire.Hosting.Dotnet;
using Microsoft.AspNetCore.Authentication.OAuth;
using Scalar.Aspire;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static class ScalarExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds a Scalar API reference to the distributed application builder with predefined theme and font settings.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder to use for authentication.</param>
        /// <returns>An <see cref="IResourceBuilder{ScalarResource}" /> configured with the specified theme and font settings.</returns>
        public IResourceBuilder<ScalarResource> AddScalar(
            IResourceBuilder<EntraIdApplicationResource>? entraId = null
        )
        {
            var scalar = builder.AddScalarApiReference(options =>
                options.DisableDefaultFonts().PreferHttpsEndpoint().AllowSelfSignedCertificates()
            );

            return entraId is null ? scalar : scalar.WithReference(entraId);
        }
    }

    extension(IResourceBuilder<ScalarResource> builder)
    {
        /// <summary>
        ///     Configures the Scalar resource builder to include an API reference with OAuth authorization.
        /// </summary>
        /// <param name="api">The .NET project resource builder representing the API project.</param>
        /// <returns>The configured Scalar resource builder with OAuth authorization.</returns>
        public IResourceBuilder<ScalarResource> WithOpenAPI(
            IResourceBuilder<DotnetProjectResource> api
        )
        {
            return builder.WithApiReference(api, async (options, ctx) =>
            {
                var clientId = api.Resource.Name;
                var entraId = builder
                    .ApplicationBuilder.Resources.OfType<EntraIdApplicationResource>()
                    .FirstOrDefault();

                if (entraId is null)
                {
                    return;
                }

                var entraClientId = await entraId.ClientIdOutputReference.GetValueAsync(ctx);
                var tenantId = await entraId.TenantIdOutputReference.GetValueAsync(ctx);
                var effectiveTenantId = string.IsNullOrWhiteSpace(tenantId) ? "common" : tenantId;
                var authority = $"{entraId.Instance.TrimEnd('/')}/{effectiveTenantId}/oauth2/v2.0";

                options
                    .AddPreferredSecuritySchemes(OAuthDefaults.DisplayName)
                    .AddAuthorizationCodeFlow(OAuthDefaults.DisplayName, flow =>
                    {
                        flow.WithPkce(Pkce.Sha256)
                            .WithAuthorizationUrl($"{authority}/authorize")
                            .WithTokenUrl($"{authority}/token");

                        if (!string.IsNullOrWhiteSpace(entraClientId))
                        {
                            flow.WithClientId(entraClientId);
                        }

                        flow.WithSelectedScopes(
                            $"{clientId}_{Authorization.Actions.Read}",
                            $"{clientId}_{Authorization.Actions.Write}"
                        );
                    });
            });
        }
    }
}
