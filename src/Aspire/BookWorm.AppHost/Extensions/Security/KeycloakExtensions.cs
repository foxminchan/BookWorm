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
    /// <remarks>
    ///     This method configures Keycloak with realm import functionality and development-specific settings:
    ///     - Imports realm configuration from <c>{BaseContainerPath}/realms</c> directory
    ///     - Only applies realm import when running in execution mode (not during build/design time)
    ///     - Configures HSTS (HTTP Strict Transport Security) based on execution context:
    ///     - <strong>Run mode:</strong> Disables HSTS to avoid browser caching issues during development
    ///     - <strong>Publish mode:</strong> Enables HSTS with 1-year max-age and includeSubDomains directive
    ///     - Sets up environment variables for realm name and display name configuration
    ///     - Addresses known Keycloak HSTS issue (https://github.com/keycloak/keycloak/issues/32366)
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var realmName = builder.AddParameter("realm-name");
    ///     var displayName = builder.AddParameter("realm-display");
    ///
    ///     builder.AddKeycloak("keycloak", 8080)
    ///            .WithSampleRealmImport(realmName, displayName);
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method enables custom theme integration for Keycloak branding and UI customization:
    ///     - Checks for themes directory existence at <c>{BaseContainerPath}/themes</c> relative to AppHost directory
    ///     - Creates bind mount from host themes directory to <c>/opt/keycloak/providers/</c> in container
    ///     - Only applies theme configuration if the themes directory exists on the host machine
    ///     - Sets read-only bind mount to prevent accidental container modifications
    ///     - Configures theme name environment variable for Keycloak theme selection
    ///     - Enables hot-reload of theme changes during development
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var themeName = builder.AddParameter("theme-name");
    ///
    ///     builder.AddKeycloak("keycloak", 8080)
    ///            .WithCustomTheme(themeName);
    ///     </code>
    /// </example>
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
    /// <param name="dbName">The database resource builder.</param>
    /// <param name="provider">The database provider type (default is "postgres").</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method configures Keycloak to connect to an external PostgreSQL database instead of using the default H2 database:
    ///     - <strong>Database type:</strong> Sets Keycloak database type to PostgreSQL
    ///     - <strong>Connection string:</strong> Constructs JDBC URL using provided host and database name
    ///     - <strong>Authentication:</strong> Configures database username and password from parameter resources
    ///     - <strong>Dependency management:</strong> Ensures Keycloak waits for database availability before starting
    ///     - <strong>Production ready:</strong> Enables persistent storage suitable for production deployments
    ///     - Replaces default embedded H2 database with external PostgreSQL for scalability and persistence
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var dbHost = ReferenceExpression.Create("my-postgres-host");
    ///     var dbUsername = builder.AddParameter("db-username");
    ///     var dbPassword = builder.AddParameter("db-password", true);
    ///     var database = builder.AddAzurePostgresFlexibleServerDatabase("keycloak-db");
    ///
    ///     builder.AddKeycloak("keycloak", 8080)
    ///            .WithExternalDatabase(dbHost, dbUsername, dbPassword, database);
    ///     </code>
    /// </example>
    public static IResourceBuilder<KeycloakResource> WithExternalDatabase(
        this IResourceBuilder<KeycloakResource> builder,
        ReferenceExpression dbHost,
        IResourceBuilder<ParameterResource> dbUsername,
        IResourceBuilder<ParameterResource> dbPassword,
        IResourceBuilder<IResourceWithConnectionString> dbName,
        string provider = "postgres"
    )
    {
        var jdbcProvider = provider switch
        {
            "postgres" or "postgresql" => "postgresql",
            "mysql" => "mysql",
            "oracle" => "oracle",
            "mariadb" => "mariadb",
            "sqlserver" => "sqlserver",
            _ => throw new ArgumentException($"Unsupported database provider: {provider}"),
        };

        return builder
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add(KeycloakDatabaseEnvVarName, provider);
                context.EnvironmentVariables.Add(KeycloakDatabaseUsernameEnvVarName, dbUsername);
                context.EnvironmentVariables.Add(KeycloakDatabasePasswordEnvVarName, dbPassword);
                context.EnvironmentVariables.Add(
                    KeycloakDatabaseUrlEnvVarName,
                    ReferenceExpression.Create(
                        $"jdbc:{jdbcProvider}://{dbHost}/{dbName.Resource.Name}"
                    )
                );
            })
            .WaitFor(dbName);
    }

    /// <summary>
    ///     Configures the Keycloak resource to run with an HTTPS development certificate.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="targetPort">The target port for the HTTPS endpoint. Defaults to 8443.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method sets up comprehensive HTTPS and HTTP configuration for Keycloak:
    ///     - <strong>Run mode only:</strong> Applies HTTPS development certificate configuration only during local development
    ///     - <strong>HTTPS setup:</strong> Configures development certificate with specified target port (default 8443)
    ///     - <strong>Hostname configuration:</strong> Sets hostname to localhost for local development
    ///     - <strong>HTTP/2 workaround:</strong> Disables HTTP/2 to prevent HTTP 431 "Header too large" errors in Keycloak
    ///     - <strong>Production settings:</strong> Enables HTTP support, configures proxy headers for load balancers
    ///     - <strong>Development optimizations:</strong> Disables hostname strict checking for flexible development
    ///     - <strong>URL visibility:</strong> Hides HTTP endpoints in summary view to encourage HTTPS usage
    ///     - Addresses known issues: https://github.com/keycloak/keycloak/discussions/10236,
    ///     https://github.com/keycloak/keycloak/issues/13933
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddKeycloak("keycloak", 8080)
    ///            .RunWithHttpsDevCertificate(8443);
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method establishes comprehensive Keycloak integration for OpenID Connect authentication:
    ///     - <strong>Client configuration:</strong> Uses project name as OAuth client ID for consistency
    ///     - <strong>Security:</strong> Generates secure 32-character client secret (no special characters)
    ///     - <strong>Environment setup:</strong> Configures Keycloak with client ID, name, secret, and URLs
    ///     - <strong>Endpoint resolution:</strong> Handles different URL formats for publish vs. run modes
    ///     - <strong>Container networking:</strong> Sets up container-host URL for internal communication
    ///     - <strong>Project integration:</strong> Configures project with Identity realm and API scopes
    ///     - <strong>Naming conventions:</strong> Transforms client names using <c>ToClientName()</c> extension
    ///     - Creates secure reference between project and Keycloak resources
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var keycloak = builder.AddKeycloak("keycloak", 8080);
    ///     var realmName = builder.AddParameter("realm-name");
    ///
    ///     builder.AddProject&lt;WebApi&gt;("api")
    ///            .WithIdP(keycloak, realmName);
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method constructs the OpenID Connect authorization endpoint for OAuth flows:
    ///     - Resolves Keycloak base URL using the specified protocol
    ///     - Constructs standard OpenID Connect authorization endpoint path
    ///     - Uses localhost for user interaction to ensure browser compatibility
    ///     - Addresses Scalar documentation generation limitations (https://github.com/scalar/scalar/issues/6225)
    ///     - Returns fully qualified URL suitable for OAuth authorization code flow
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var authUrl = keycloak.GetAuthorizationEndpoint(Protocol.Https, realmName);
    ///     // Returns: https://localhost:8443/realms/my-realm/protocol/openid-connect/auth
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method constructs the OpenID Connect token endpoint for OAuth flows:
    ///     - Uses Keycloak resource name instead of resolved URL for proxy configuration
    ///     - Constructs standard OpenID Connect token endpoint path
    ///     - Optimized for internal service-to-service communication within container networks
    ///     - Addresses Scalar documentation generation limitations (https://github.com/scalar/scalar/issues/6225)
    ///     - Returns URL suitable for OAuth token exchange and refresh operations
    ///     - Enables proper proxy routing in containerized environments
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var tokenUrl = keycloak.GetTokenEndpoint(Protocol.Http, realmName);
    ///     // Returns: http://keycloak/realms/myrealm/protocol/openid-connect/token
    ///     </code>
    /// </example>
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
