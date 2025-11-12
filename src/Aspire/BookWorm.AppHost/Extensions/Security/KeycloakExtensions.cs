namespace BookWorm.AppHost.Extensions.Security;

public static class KeycloakExtensions
{
    private const string BaseContainerPath = "Container/keycloak";
    private const string RealmName = "REALM_NAME";
    private const string RealmHsts = "REALM_HSTS";
    private const string ThemeName = "THEME_NAME";
    private const string RealmDisplayName = "REALM_DISPLAY_NAME";
    private const string HttpEnabledEnvVarName = "KC_HTTP_ENABLED";
    private const string ProxyHeadersEnvVarName = "KC_PROXY_HEADERS";
    private const string HostNameStrictEnvVarName = "KC_HOSTNAME_STRICT";
    private const string KeycloakDatabaseEnvVarName = "KC_DB";
    private const string KeycloakDatabaseUsernameEnvVarName = "KC_DB_USERNAME";
    private const string KeycloakDatabasePasswordEnvVarName = "KC_DB_PASSWORD";
    private const string KeycloakTransactionXaEnabledEnvVarName = "KC_TRANSACTION_XA_ENABLED";
    private const string KeycloakDatabaseUrlEnvVarName = "KC_DB_URL";
    private static readonly string _defaultLocalKeycloakName = nameof(BookWorm).ToLowerInvariant();

    /// <param name="builder">The distributed application builder to which the Keycloak resource will be added.</param>
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds a Keycloak container resource to the distributed application builder with custom theme and realm import
        ///     settings.
        /// </summary>
        /// <param name="name">The name of the Keycloak resource.</param>
        /// <returns>
        ///     An <see cref="IResourceBuilder{KeycloakResource}" /> representing the configured Keycloak resource.
        /// </returns>
        public IResourceBuilder<KeycloakResource> AddLocalKeycloak(string name)
        {
            var keycloak = builder
                .AddKeycloak(name)
                .WithIconName("LockClosedRibbon")
                .WithCustomTheme(_defaultLocalKeycloakName)
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithSampleRealmImport(_defaultLocalKeycloakName, nameof(BookWorm));

            return keycloak;
        }

        /// <summary>
        ///     Adds a hosted Keycloak external service to the distributed application builder.
        /// </summary>
        /// <param name="name">The name of the Keycloak external service resource.</param>
        /// <returns>
        ///     An <see cref="IResourceBuilder{ExternalServiceResource}" /> representing the configured Keycloak external service.
        /// </returns>
        public IResourceBuilder<ExternalServiceResource> AddHostedKeycloak(string name)
        {
            var keycloakUrl = builder
                .AddParameter("kc-url", true)
                .WithDescription(ParameterDescriptions.Keycloak.Url, true)
                .WithCustomInput(_ =>
                    new()
                    {
                        Name = "KeycloakUrlParameter",
                        Label = "Keycloak URL",
                        InputType = InputType.Text,
                        Value = "https://identity.bookworm.com",
                        Description = "Enter your Keycloak server URL here",
                    }
                );

            builder
                .AddParameter("kc-realm", true)
                .WithDescription(ParameterDescriptions.Keycloak.Realm, true)
                .WithCustomInput(_ =>
                    new()
                    {
                        Name = "KeycloakRealmParameter",
                        Label = "Keycloak Realm",
                        InputType = InputType.Text,
                        Value = nameof(BookWorm).ToLowerInvariant(),
                        Description = "Enter your Keycloak realm name here",
                    }
                );

            var keycloak = builder
                .AddExternalService(name, keycloakUrl)
                .WithIconName("LockClosedRibbon")
                .WithHttpHealthCheck("/health/ready");

            keycloakUrl.WithParentRelationship(keycloak);

            return keycloak;
        }
    }

    /// <summary>
    ///     Configures a <see cref="KeycloakResource" /> to use an external PostgreSQL database.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{KeycloakResource}" /> instance.</param>
    /// <param name="postgres">The PostgreSQL database resource builder.</param>
    /// <param name="xaEnabled">Whether to enable XA transactions. Default is false.</param>
    /// <returns>The <see cref="IResourceBuilder{KeycloakResource}" /> for method chaining.</returns>
    public static IResourceBuilder<KeycloakResource> WithPostgres(
        this IResourceBuilder<KeycloakResource> builder,
        IResourceBuilder<AzurePostgresFlexibleServerResource> postgres,
        bool xaEnabled = false
    )
    {
        var userDb = postgres.AddDatabase(Components.Database.User);

        return builder
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add(KeycloakDatabaseEnvVarName, "postgres");
                context.EnvironmentVariables.Add(
                    KeycloakDatabaseUsernameEnvVarName,
                    userDb.Resource.Parent.UserName ?? ReferenceExpression.Create($"postgres")
                );
                context.EnvironmentVariables.Add(
                    KeycloakDatabasePasswordEnvVarName,
                    userDb.Resource.Parent.Password ?? ReferenceExpression.Create($"postgres")
                );
                context.EnvironmentVariables.Add(
                    KeycloakDatabaseUrlEnvVarName,
                    ReferenceExpression.Create(
                        $"jdbc:postgresql://{userDb.Resource.Parent.HostName}/{userDb.Resource.DatabaseName}"
                    )
                );
                context.EnvironmentVariables.Add(
                    KeycloakTransactionXaEnabledEnvVarName,
                    xaEnabled.ToString().ToLowerInvariant()
                );
            })
            .WaitFor(userDb);
    }

    /// <summary>
    ///     Configures the project resource to integrate with Keycloak as an Identity Provider (IdP).
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="keycloak">The Keycloak resource builder to configure as an IdP.</param>
    /// <returns>The project resource builder for method chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithKeycloak(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<IResource> keycloak
    )
    {
        var clientId = builder.Resource.Name;
        var applicationBuilder = builder.ApplicationBuilder;

        if (
            applicationBuilder.ExecutionContext.IsRunMode
            && keycloak is IResourceBuilder<KeycloakResource> keycloakContainer
        )
        {
            var clientSecret = applicationBuilder
                .AddParameter($"{clientId}-secret", true)
                .WithGeneratedDefault(new() { MinLength = 32, Special = false });

            var clientEnv = clientId.ToUpperInvariant();

            keycloakContainer
                .WithEnvironment(HttpEnabledEnvVarName, "true")
                .WithEnvironment(ProxyHeadersEnvVarName, "xforwarded")
                .WithEnvironment(HostNameStrictEnvVarName, "false")
                .WithEnvironment($"CLIENT_{clientEnv}_ID", clientId)
                .WithEnvironment($"CLIENT_{clientEnv}_NAME", clientId.ToClientName())
                .WithEnvironment($"CLIENT_{clientEnv}_SECRET", clientSecret)
                .OnResourceEndpointsAllocated(
                    (resource, _, _) =>
                    {
                        var resourceBuilder = builder.ApplicationBuilder.CreateResourceBuilder(
                            resource
                        );

                        resourceBuilder.WithEnvironment(context =>
                        {
                            var endpoint = builder.GetEndpoint(Protocols.Http);

                            context.EnvironmentVariables.Add(
                                $"CLIENT_{clientEnv}_URL",
                                endpoint.Url
                            );

                            context.EnvironmentVariables.Add(
                                $"CLIENT_{clientEnv}_URL_CONTAINERHOST",
                                endpoint
                            );
                        });

                        return Task.CompletedTask;
                    }
                );

            builder
                .WithReference(keycloakContainer)
                .WaitForStart(keycloakContainer)
                .WithEnvironment("Identity__Realm", _defaultLocalKeycloakName)
                .WithEnvironment("Identity__ClientId", clientId)
                .WithEnvironment("Identity__ClientSecret", clientSecret)
                .WithEnvironment(
                    $"Identity__Scopes__{clientId}_{Authorization.Actions.Read}",
                    $"{nameof(Authorization.Actions.Read)} for {clientId.ToClientName("API")}"
                )
                .WithEnvironment(
                    $"Identity__Scopes__{clientId}_{Authorization.Actions.Write}",
                    $"{nameof(Authorization.Actions.Write)} for {clientId.ToClientName("API")}"
                );
        }
        else if (
            applicationBuilder.ExecutionContext.IsPublishMode
            && keycloak is IResourceBuilder<ExternalServiceResource> keycloakHosted
        )
        {
            var clientSecret = applicationBuilder
                .AddParameter($"{clientId}-secret", true)
                .WithDescription(ParameterDescriptions.Keycloak.ClientSecret, true)
                .WithCustomInput(_ =>
                    new()
                    {
                        Name = "KeycloakClientSecretParameter",
                        Label = "Keycloak Client Secret",
                        InputType = InputType.SecretText,
                        Description = "Enter your Keycloak client secret here",
                    }
                );

            var realmParameter = applicationBuilder
                .Resources.OfType<ParameterResource>()
                .First(r => string.Equals(r.Name, "kc-realm", StringComparison.OrdinalIgnoreCase));

            builder
                .WithReference(keycloakHosted)
                .WaitFor(keycloakHosted)
                .WithEnvironment("Identity__Realm", realmParameter)
                .WithEnvironment("Identity__ClientId", clientId)
                .WithEnvironment("Identity__ClientSecret", clientSecret)
                .WithEnvironment($"Identity__Scopes__{clientId}", clientId.ToClientName("API"));
        }

        return builder;
    }

    extension(IResourceBuilder<KeycloakResource> builder)
    {
        private IResourceBuilder<KeycloakResource> WithSampleRealmImport(
            string realmName,
            string displayName
        )
        {
            builder
                .WithRealmImport($"{BaseContainerPath}/realms")
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

        private IResourceBuilder<KeycloakResource> WithCustomTheme(string themeName)
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
    }
}
