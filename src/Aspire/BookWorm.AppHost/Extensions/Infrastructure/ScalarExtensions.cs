using Microsoft.AspNetCore.Authentication.OAuth;
using Scalar.Aspire;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class ScalarExtensions
{
    private static readonly Dictionary<
        IDistributedApplicationBuilder,
        (IResourceBuilder<KeycloakResource> Keycloak, IResourceBuilder<ParameterResource> RealmName)
    > _cachedResources = [];

    /// <summary>
    ///     Adds a Scalar API reference to the distributed application builder with predefined theme and font settings.
    /// </summary>
    /// <param name="builder">The distributed application builder to extend.</param>
    /// <param name="keycloak">The Keycloak resource builder to use for authentication.</param>
    /// <returns>An <see cref="IResourceBuilder{ScalarResource}" /> configured with the specified theme and font settings.</returns>
    /// <remarks>
    ///     This method creates a Scalar API documentation resource with the following configuration:
    ///     - Sets the theme to BluePlanet for visual consistency
    ///     - Disables default fonts to allow custom font configuration
    ///     - Establishes a reference to the provided Keycloak resource for authentication
    ///     - Provides a foundation for API documentation with OAuth integration
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var keycloak = builder.AddKeycloak("keycloak", 8080);
    ///     var scalar = builder.AddScalar(keycloak);
    ///     </code>
    /// </example>
    public static IResourceBuilder<ScalarResource> AddScalar(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<KeycloakResource> keycloak
    )
    {
        // If the Keycloak resource is running in HTTPS container, please remove the WaitFor() call.
        // https://github.com/dotnet/aspire/issues/6890
        return builder
            .AddScalarApiReference(options =>
                options.WithTheme(ScalarTheme.BluePlanet).WithDefaultFonts(false)
            )
            .WithReference(keycloak)
            .WaitFor(keycloak);
    }

    /// <summary>
    ///     Configures the Scalar resource builder to include an API reference with OAuth authorization.
    /// </summary>
    /// <param name="builder">The Scalar resource builder to configure.</param>
    /// <param name="api">The project resource builder representing the API project.</param>
    /// <returns>The configured Scalar resource builder with OAuth authorization.</returns>
    /// <remarks>
    ///     This method establishes comprehensive OAuth integration for API documentation:
    ///     - Configures OAuth as the preferred security scheme using <c>OAuthDefaults.DisplayName</c>
    ///     - Sets up Authorization Code flow with PKCE (SHA256) for enhanced security
    ///     - Uses the API project name as the OAuth client ID
    ///     - Automatically locates and uses client secret parameter if available (format: "{clientId}-secret")
    ///     - Retrieves Keycloak authorization and token endpoints from cached resources
    ///     - Configures selected scopes to match the client ID for proper access control
    ///     - Caches Keycloak and realm resources for performance optimization
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var api = builder.AddProject&lt;WebApi&gt;("api");
    ///     var scalar = builder.AddScalar(keycloak)
    ///                         .WithOpenAPI(api);
    ///     </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when Keycloak resource is not found in the application builder or when the required 'kc-realm' parameter is
    ///     not configured.
    /// </exception>
    public static IResourceBuilder<ScalarResource> WithOpenAPI(
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

        var (keycloak, realmName) = GetCachedResources(builder.ApplicationBuilder);

        return builder.WithApiReference(
            api,
            options =>
            {
                options
                    .AddPreferredSecuritySchemes(OAuthDefaults.DisplayName)
                    .AddAuthorizationCodeFlow(
                        OAuthDefaults.DisplayName,
                        flow =>
                        {
                            flow.WithPkce(Pkce.Sha256)
                                .WithAuthorizationUrl(
                                    keycloak.GetAuthorizationEndpoint(Protocol.Http, realmName)
                                )
                                .WithTokenUrl(keycloak.GetTokenEndpoint(Protocol.Http, realmName))
                                .WithClientId(clientId);

                            if (secret is not null)
                            {
                                flow.WithClientSecret(secret);
                            }

                            flow.WithSelectedScopes(clientId);
                        }
                    );
            }
        );
    }

    private static (
        IResourceBuilder<KeycloakResource> Keycloak,
        IResourceBuilder<ParameterResource> RealmName
    ) GetCachedResources(IDistributedApplicationBuilder builder)
    {
        if (_cachedResources.TryGetValue(builder, out var cached))
        {
            return cached;
        }

        var keycloak =
            builder
                .Resources.OfType<KeycloakResource>()
                .Select(builder.CreateResourceBuilder)
                .FirstOrDefault()
            ?? throw new InvalidOperationException("Keycloak resource not found.");

        var realmName =
            builder
                .Resources.OfType<ParameterResource>()
                .FirstOrDefault(r =>
                    string.Equals(r.Name, "kc-realm", StringComparison.OrdinalIgnoreCase)
                )
            ?? throw new InvalidOperationException(
                "Keycloak realm parameter 'kc-realm' not found."
            );

        var realmNameBuilder = builder.CreateResourceBuilder(realmName);

        var result = (keycloak, realmNameBuilder);
        _cachedResources[builder] = result;
        return result;
    }
}
