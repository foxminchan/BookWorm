﻿namespace BookWorm.AppHost.Extensions.Security;

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
    private const string KeycloakHttpsCertFileEnvVarName = "KC_HTTPS_CERTIFICATE_FILE";
    private const string KeycloakHttpsCertKeyFileEnvVarName = "KC_HTTPS_CERTIFICATE_KEY_FILE";
    private const string KeycloakDatabaseEnvVarName = "KC_DB";
    private const string KeycloakDatabaseUsernameEnvVarName = "KC_DB_USERNAME";
    private const string KeycloakDatabasePasswordEnvVarName = "KC_DB_PASSWORD";
    private const string KeycloakDatabaseUrlEnvVarName = "KC_DB_URL";

    /// <summary>
    ///     Configures a Keycloak resource with a sample realm import.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="realmName">The name of the realm to import.</param>
    /// <param name="displayName">The display name of the realm.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    public static IResourceBuilder<KeycloakResource> WithSampleRealmImport(
        this IResourceBuilder<KeycloakResource> builder,
        IResourceBuilder<ParameterResource> realmName,
        IResourceBuilder<ParameterResource> displayName
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
    public static IResourceBuilder<KeycloakResource> WithCustomTheme(
        this IResourceBuilder<KeycloakResource> builder,
        IResourceBuilder<ParameterResource> themeName
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
    ///     Configures a Keycloak resource to use an external PostgreSQL database.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="dbHost">The database host reference expression.</param>
    /// <param name="dbUsername">The database username parameter resource builder.</param>
    /// <param name="dbPassword">The database password parameter resource builder.</param>
    /// <param name="dbSchema">The database resource builder.</param>
    /// <param name="dbProvider">The database provider type (default is "postgres").</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    public static IResourceBuilder<KeycloakResource> WithExternalDatabase(
        this IResourceBuilder<KeycloakResource> builder,
        ReferenceExpression dbHost,
        IResourceBuilder<ParameterResource> dbUsername,
        IResourceBuilder<ParameterResource> dbPassword,
        IResourceBuilder<IResourceWithConnectionString> dbSchema,
        string dbProvider = "postgres"
    )
    {
        var dbType = dbProvider switch
        {
            "postgres" or "postgresql" => "postgresql",
            "mysql" => "mysql",
            "oracle" => "oracle",
            "mariadb" => "mariadb",
            "sqlserver" => "sqlserver",
            _ => throw new ArgumentException($"Unsupported database provider: {dbProvider}"),
        };

        return builder
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add(KeycloakDatabaseEnvVarName, dbProvider);
                context.EnvironmentVariables.Add(KeycloakDatabaseUsernameEnvVarName, dbUsername);
                context.EnvironmentVariables.Add(KeycloakDatabasePasswordEnvVarName, dbPassword);
                context.EnvironmentVariables.Add(
                    KeycloakDatabaseUrlEnvVarName,
                    ReferenceExpression.Create($"jdbc:{dbType}://{dbHost}/{dbSchema.Resource.Name}")
                );
            })
            .WaitFor(dbSchema);
    }

    /// <summary>
    ///     Configures the Keycloak resource to run with an HTTPS development certificate.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="targetPort">The target port for the HTTPS endpoint. Defaults to 8443.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    public static IResourceBuilder<KeycloakResource> RunWithHttpsDevCertificate(
        this IResourceBuilder<KeycloakResource> builder,
        int targetPort = 8443
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder
                .RunWithHttpsDevCertificate(
                    KeycloakHttpsCertFileEnvVarName,
                    KeycloakHttpsCertKeyFileEnvVarName
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
    /// <param name="realmName">The realm name parameter resource builder.</param>
    /// <returns>The project resource builder for method chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithIdP(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<ParameterResource> realmName
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
                var endpoint = builder.GetEndpoint(applicationBuilder.GetLaunchProfileName());

                context.EnvironmentVariables[$"CLIENT_{clientEnv}_URL"] = context
                    .ExecutionContext
                    .IsPublishMode
                    ? endpoint
                    : endpoint.Url;
                context.EnvironmentVariables[$"CLIENT_{clientEnv}_URL_CONTAINERHOST"] = endpoint;
            });

        // If the Keycloak resource is running in HTTPS container, please remove the WaitFor() call.
        // https://github.com/dotnet/aspire/issues/6890
        return builder
            .WithReference(keycloak)
            .WaitFor(keycloak)
            .WithEnvironment("Identity__Realm", realmName)
            .WithEnvironment("Identity__ClientId", clientId)
            .WithEnvironment("Identity__ClientSecret", clientSecret)
            .WithEnvironment($"Identity__Scopes__{clientId}", clientId.ToClientName("API"));
    }

    /// <summary>
    ///     Builds the OpenID Connect authorization endpoint URL for the specified Keycloak realm.
    ///     Uses localhost for user interaction to address Scalar documentation generation issues.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IResourceBuilder{KeycloakResource}" /> used to resolve the Keycloak endpoint.
    /// </param>
    /// <param name="realmName">
    ///     The <see cref="IResourceBuilder{ParameterResource}" /> containing the realm name.
    /// </param>
    /// <param name="protocol">Specifies the protocol to use (e.g., "http" or "https").</param>
    /// <returns>
    ///     The authorization endpoint URL for the given realm.
    /// </returns>
    public static string GetAuthorizationEndpoint(
        this IResourceBuilder<KeycloakResource> builder,
        string protocol,
        IResourceBuilder<ParameterResource> realmName
    )
    {
        var keycloakUrl = builder.GetEndpoint(protocol).Url;

        var realm = realmName.Resource.Value;

        return $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/auth";
    }

    /// <summary>
    ///     Builds the OpenID Connect token endpoint URL for the specified Keycloak realm.
    ///     Uses the Keycloak resource name for proxy configuration to address Scalar documentation generation issues.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IResourceBuilder{KeycloakResource}" /> used to resolve the Keycloak endpoint.
    /// </param>
    /// <param name="realmName">
    ///     The <see cref="IResourceBuilder{ParameterResource}" /> containing the realm name.
    /// </param>
    /// <param name="protocol">Specifies the protocol to use (e.g., "http" or "https").</param>
    /// <returns>
    ///     The token endpoint URL for the given realm.
    /// </returns>
    public static string GetTokenEndpoint(
        this IResourceBuilder<KeycloakResource> builder,
        string protocol,
        IResourceBuilder<ParameterResource> realmName
    )
    {
        var realm = realmName.Resource.Value;

        return $"{protocol}://{builder.Resource.Name}/realms/{realm}/protocol/openid-connect/token";
    }
}
