namespace BookWorm.AppHost.Extensions.Security;

public static class KeycloakExtensions
{
    private const string BaseContainerPath = "Container/keycloak";
    private const string RealmName = "REALM_NAME";
    private const string RealmDisplayName = "REALM_DISPLAY_NAME";
    private const string RealmHsts = "REALM_HSTS";
    private const string ThemeName = "THEME_NAME";
    private const string HttpEnabledEnvVarName = "KC_HTTP_ENABLED";
    private const string ProxyHeadersEnvVarName = "KC_PROXY_HEADERS";
    private const string KeycloakHttpsPortEnvVarName = "KC_HTTPS_PORT";
    private const string KeycloakHostnameEnvVarName = "KC_HOSTNAME";
    private const string HostNameStrictEnvVarName = "KC_HOSTNAME_STRICT";
    private const string QuarkusHttp2EnvVarName = "QUARKUS_HTTP_HTTP2";
    private const string KeyCloakHttpsCertFileEnvVarName = "KC_HTTPS_CERTIFICATE_FILE";
    private const string KeyCloakHttpsCertKeyFileEnvVarName = "KC_HTTPS_CERTIFICATE_KEY_FILE";

    /// <summary>
    ///     Configures a Keycloak resource with a sample realm import.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="realmName">The name of the realm to import.</param>
    /// <param name="displayName">The display name of the realm.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This extension only imports the realm when running in execution mode,
    ///     not during build or design time.
    /// </remarks>
    public static IResourceBuilder<KeycloakResource> WithSampleRealmImport(
        this IResourceBuilder<KeycloakResource> builder,
        string realmName,
        string displayName
    )
    {
        builder
            .WithRealmImport($"{BaseContainerPath}/realms", true)
            .WithEnvironment(RealmName, realmName)
            .WithEnvironment(RealmDisplayName, displayName)
            // Ensure HSTS is not enabled in run mode to avoid browser caching issues when developing.
            // Workaround for https://github.com/keycloak/keycloak/issues/32366
            .WithEnvironment(
                RealmHsts,
                builder.ApplicationBuilder.ExecutionContext.IsRunMode
                    ? string.Empty
                    : "max-age=31536000; includeSubDomains"
            );

        return builder;
    }

    /// <summary>
    ///     Configures a Keycloak resource with a custom theme.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="themeName">The name of the custom theme to use.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This extension mounts the custom themes directory from the host machine
    ///     to the Keycloak container's providers directory.
    ///     The mount is only performed if the themes directory exists on the host machine.
    /// </remarks>
    public static IResourceBuilder<KeycloakResource> WithCustomTheme(
        this IResourceBuilder<KeycloakResource> builder,
        string themeName
    )
    {
        var importFullPath = Path.GetFullPath(
            $"{BaseContainerPath}/themes",
            builder.ApplicationBuilder.AppHostDirectory
        );

        if (Directory.Exists(importFullPath))
        {
            builder
                .WithBindMount(importFullPath, "/opt/keycloak/providers/", true)
                .WithEnvironment(ThemeName, themeName);
        }

        return builder;
    }

    /// <summary>
    ///     Configures the Keycloak resource to run with an HTTPS development certificate.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="targetPort">The target port for the HTTPS endpoint. Defaults to 8443.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method sets up Keycloak to use a development HTTPS certificate when running in execution mode.
    ///     It also configures necessary environment variables to avoid common issues such as HTTP 431 errors.
    /// </remarks>
    public static IResourceBuilder<KeycloakResource> RunWithHttpsDevCertificate(
        this IResourceBuilder<KeycloakResource> builder,
        int targetPort = 8443
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder
                .RunWithHttpsDevCertificate(
                    KeyCloakHttpsCertFileEnvVarName,
                    KeyCloakHttpsCertKeyFileEnvVarName
                )
                .WithHttpsEndpoint(env: KeycloakHttpsPortEnvVarName, targetPort: targetPort)
                .WithEnvironment(KeycloakHostnameEnvVarName, "localhost")
                // Without disabling HTTP/2, you can hit HTTP 431 Header too large errors in Keycloak.
                // Related issues:
                // https://github.com/keycloak/keycloak/discussions/10236
                // https://github.com/keycloak/keycloak/issues/13933
                // https://github.com/quarkusio/quarkus/issues/33692
                .WithEnvironment(QuarkusHttp2EnvVarName, "false");
        }

        return builder
            .WithEnvironment(HttpEnabledEnvVarName, "true")
            .WithEnvironment(ProxyHeadersEnvVarName, "xforwarded")
            .WithEnvironment(HostNameStrictEnvVarName, "false")
            .WithUrlForEndpoint(
                Protocol.Http,
                url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly
            );
    }

    /// <summary>
    ///     Configures the project resource to integrate with Keycloak as an Identity Provider (IdP).
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="keycloak">The Keycloak resource builder to configure as an IdP.</param>
    /// <returns>The project resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method sets up environment variables required for Keycloak integration,
    ///     including client ID, client secret, and URLs.
    ///     It generates a secure client secret and configures endpoints based on the application's launch profile.
    /// </remarks>
    public static IResourceBuilder<ProjectResource> WithIdP(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<KeycloakResource> keycloak
    )
    {
        var clientId = builder.Resource.Name;
        var applicationBuilder = builder.ApplicationBuilder;
        var clientSecret = applicationBuilder
            .AddParameter($"{clientId}-secret", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        var clientEnv = clientId.ToUpperInvariant();

        keycloak
            .WithEnvironment($"CLIENT_{clientEnv}_ID", clientId)
            .WithEnvironment($"CLIENT_{clientEnv}_NAME", clientId.ToClientName())
            .WithEnvironment($"CLIENT_{clientEnv}_SECRET", clientSecret)
            .WithEnvironment(context =>
            {
                var endpoint = builder.GetEndpoint(
                    applicationBuilder.IsHttpsLaunchProfile() ? Protocol.Https : Protocol.Http
                );

                context.EnvironmentVariables[$"CLIENT_{clientEnv}_URL"] = context
                    .ExecutionContext
                    .IsPublishMode
                    ? endpoint
                    : endpoint.Url;
                context.EnvironmentVariables[$"CLIENT_{clientEnv}_URL_CONTAINERHOST"] = endpoint;
            });

        return builder
            .WithReference(keycloak)
            .WithEnvironment("Identity__ClientId", clientId)
            .WithEnvironment("Identity__ClientSecret", clientSecret);
    }
}
