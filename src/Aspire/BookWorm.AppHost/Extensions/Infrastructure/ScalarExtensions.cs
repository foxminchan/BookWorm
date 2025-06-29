using Microsoft.AspNetCore.Authentication.OAuth;
using Scalar.Aspire;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class ScalarExtensions
{
    /// <summary>
    ///     Adds a Scalar API reference to the distributed application builder with predefined theme and font settings.
    /// </summary>
    /// <param name="builder">The distributed application builder to extend.</param>
    /// <returns>An <see cref="IResourceBuilder{ScalarResource}" /> configured with the specified theme and font settings.</returns>
    public static IResourceBuilder<ScalarResource> AddScalar(
        this IDistributedApplicationBuilder builder
    )
    {
        return builder.AddScalarApiReference(options =>
            options.WithTheme(ScalarTheme.BluePlanet).WithDefaultFonts(false)
        );
    }

    /// <summary>
    ///     Configures the Scalar resource builder to include an API reference with OAuth authorization.
    /// </summary>
    /// <param name="builder">The Scalar resource builder to configure.</param>
    /// <param name="api">The project resource builder representing the API project.</param>
    /// <returns>The configured Scalar resource builder with OAuth authorization.</returns>
    public static IResourceBuilder<ScalarResource> WithApi(
        this IResourceBuilder<ScalarResource> builder,
        IResourceBuilder<ProjectResource> api
    )
    {
        var clientId = api.Resource.Name;

        var secret = builder
            .ApplicationBuilder.Resources.OfType<ParameterResource>()
            .FirstOrDefault(r =>
                string.Equals(r.Name, $"{clientId}-secret", StringComparison.OrdinalIgnoreCase)
            )
            ?.Value;

        return builder.WithApiReference(
            api,
            options =>
                options
                    .AddPreferredSecuritySchemes(OAuthDefaults.DisplayName)
                    .AddAuthorizationCodeFlow(
                        OAuthDefaults.DisplayName,
                        flow =>
                            flow.WithPkce(Pkce.Sha256)
                                .WithClientId(clientId)
                                .WithClientSecret(secret)
                                .WithSelectedScopes(clientId)
                    )
        );
    }
}
