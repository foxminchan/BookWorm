using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Authentication.OAuth;
using Scalar.Aspire;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class ScalarExtensions
{
    /// <summary>
    ///     Adds a Scalar API reference to the distributed application builder with predefined theme and font settings.
    /// </summary>
    /// <param name="builder">The distributed application builder to extend.</param>
    /// <param name="keycloak">The Keycloak resource builder to use for authentication.</param>
    /// <returns>An <see cref="IResourceBuilder{ScalarResource}" /> configured with the specified theme and font settings.</returns>
    public static IResourceBuilder<ScalarResource> AddScalar(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResource>? keycloak = null
    )
    {
        // https://github.com/dotnet/aspire/issues/6890
        var scalar = builder.AddScalarApiReference(options =>
            options.WithDefaultFonts(false).PreferHttpsEndpoint().AllowSelfSignedCertificates()
        );

        if (keycloak is null)
        {
            return scalar;
        }

        return keycloak switch
        {
            IResourceBuilder<ExternalServiceResource> externalService => scalar
                .WithReference(externalService)
                .WaitFor(externalService),

            IResourceBuilder<KeycloakResource> container => scalar
                .WithReference(container)
                .WaitForStart(container),

            _ => throw new InvalidOperationException("Unsupported Keycloak resource type"),
        };
    }

    /// <summary>
    ///     Configures the Scalar resource builder to include an API reference with OAuth authorization.
    /// </summary>
    /// <param name="builder">The Scalar resource builder to configure.</param>
    /// <param name="api">The project resource builder representing the API project.</param>
    /// <returns>The configured Scalar resource builder with OAuth authorization.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when Keycloak resource is not found in the application builder or when the required 'kc-realm' parameter is
    ///     not configured.
    /// </exception>
    public static IResourceBuilder<ScalarResource> WithOpenAPI(
        this IResourceBuilder<ScalarResource> builder,
        IResourceBuilder<ProjectResource> api
    )
    {
        return builder.WithApiReference(
            api,
            async (options, ctx) =>
            {
                var clientId = api.Resource.Name;

                var parameter = builder
                    .ApplicationBuilder.Resources.OfType<ParameterResource>()
                    .FirstOrDefault(r =>
                        string.Equals(
                            r.Name,
                            $"{clientId}-secret",
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

                if (parameter is not null)
                {
                    var clientSecret = await parameter.GetValueAsync(ctx);

                    options
                        .AddPreferredSecuritySchemes(OAuthDefaults.DisplayName)
                        .AddAuthorizationCodeFlow(
                            OAuthDefaults.DisplayName,
                            flow =>
                            {
                                flow.WithPkce(Pkce.Sha256)
                                    .WithClientId(clientId)
                                    .AddBodyParameter("audience", "account");

                                if (!string.IsNullOrWhiteSpace(clientSecret))
                                {
                                    flow.WithClientSecret(clientSecret);
                                }

                                flow.WithSelectedScopes(
                                    $"{clientId}_{Authorization.Actions.Read}",
                                    $"{clientId}_{Authorization.Actions.Write}"
                                );
                            }
                        );
                }
            }
        );
    }
}
