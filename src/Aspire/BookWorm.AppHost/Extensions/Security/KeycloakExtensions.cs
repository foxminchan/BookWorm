namespace BookWorm.AppHost.Extensions.Security;

public static class KeycloakExtensions
{
    private const string ThemeName = "THEME_NAME";
    private const string RealmName = "REALM_NAME";
    private const string RealmDisplayName = "REALM_DISPLAY_NAME";
    private const string HttpEnabledEnvVarName = "KC_HTTP_ENABLED";
    private const string ProxyHeadersEnvVarName = "KC_PROXY_HEADERS";
    private const string HostNameStrictEnvVarName = "KC_HOSTNAME_STRICT";
    private const string BaseContainerPath = "Container/keycloak";
    private static readonly string _defaultLocalKeycloakName = nameof(BookWorm).ToLowerInvariant();

    /// <summary>
    ///     Configures the turborepo app resource to integrate with Keycloak as an Identity Provider (IdP).
    /// </summary>
    /// <param name="builder">The turborepo app resource builder.</param>
    /// <param name="keycloak">The Keycloak resource builder to configure as an IdP.</param>
    /// <returns>The turborepo app resource builder for method chaining.</returns>
    public static IResourceBuilder<TurborepoAppResource> WithKeycloak(
        this IResourceBuilder<TurborepoAppResource> builder,
        IResourceBuilder<IResource> keycloak
    )
    {
        var clientId = builder.Resource.Name;

        var betterAuthSecret = builder
            .ApplicationBuilder.AddParameter($"{clientId}-better-auth-secret", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        switch (keycloak)
        {
            case IResourceBuilder<KeycloakResource> keycloakContainer:
                ConfigureKeycloakForClient(
                    keycloakContainer,
                    builder,
                    clientId,
                    clientType: "APP",
                    clientSecret: null,
                    includeContainerHostUrl: false
                );

                builder
                    .WithReference(keycloakContainer)
                    .WaitForStart(keycloakContainer)
                    .WithEnvironment("BETTER_AUTH_SECRET", betterAuthSecret)
                    .WithEnvironment(
                        "KEYCLOAK_URL",
                        keycloakContainer.GetEndpoint(Http.Schemes.Http)
                    )
                    .WithEnvironment("KEYCLOAK_REALM", _defaultLocalKeycloakName)
                    .WithEnvironment("KEYCLOAK_CLIENT_ID", clientId);
                break;
            case IResourceBuilder<ExternalServiceResource> keycloakHosted:
                ConfigureClientForHostedKeycloak(
                    builder,
                    keycloakHosted,
                    betterAuthSecret,
                    clientId
                );
                break;
        }

        return builder;
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

        switch (keycloak)
        {
            case IResourceBuilder<KeycloakResource> keycloakContainer:
            {
                var clientSecret = applicationBuilder
                    .AddParameter($"{clientId}-secret", true)
                    .WithGeneratedDefault(new() { MinLength = 32, Special = false });

                ConfigureKeycloakForClient(
                    keycloakContainer,
                    builder,
                    clientId,
                    clientType: "API",
                    clientSecret,
                    includeContainerHostUrl: true
                );

                builder
                    .WithReference(keycloakContainer)
                    .WaitForStart(keycloakContainer)
                    .WithEnvironment("Identity__Realm", _defaultLocalKeycloakName)
                    .WithEnvironment("Identity__ClientId", clientId)
                    .WithEnvironment("Identity__ClientSecret", clientSecret)
                    .WithEnvironment(
                        $"Identity__Scopes__{clientId}_{Authorization.Actions.Read}",
                        $"{nameof(Authorization.Actions.Read)} for {Services.ToClientName(clientId, "API")}"
                    )
                    .WithEnvironment(
                        $"Identity__Scopes__{clientId}_{Authorization.Actions.Write}",
                        $"{nameof(Authorization.Actions.Write)} for {Services.ToClientName(clientId, "API")}"
                    );
                break;
            }
            case IResourceBuilder<ExternalServiceResource> keycloakHosted:
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
                    .First(r =>
                        string.Equals(r.Name, "kc-realm", StringComparison.OrdinalIgnoreCase)
                    );

                builder
                    .WithReference(keycloakHosted)
                    .WaitFor(keycloakHosted)
                    .WithEnvironment("Identity__Realm", realmParameter)
                    .WithEnvironment("Identity__ClientId", clientId)
                    .WithEnvironment("Identity__ClientSecret", clientSecret)
                    .WithEnvironment(
                        $"Identity__Scopes__{clientId}",
                        Services.ToClientName(clientId, "API")
                    );
                break;
            }
        }

        return builder;
    }

    private static void ConfigureKeycloakForClient<TResource>(
        IResourceBuilder<KeycloakResource> keycloakContainer,
        IResourceBuilder<TResource> clientBuilder,
        string clientId,
        string clientType,
        IResourceBuilder<ParameterResource>? clientSecret,
        bool includeContainerHostUrl
    )
        where TResource : IResourceWithEndpoints
    {
        var clientEnv = clientId.ToUpperInvariant();

        keycloakContainer
            .WithEnvironment(HttpEnabledEnvVarName, "true")
            .WithEnvironment(ProxyHeadersEnvVarName, "xforwarded")
            .WithEnvironment(HostNameStrictEnvVarName, "false")
            .WithEnvironment($"CLIENT_{clientEnv}_ID", clientId)
            .WithEnvironment(
                $"CLIENT_{clientEnv}_NAME",
                Services.ToClientName(clientId, clientType)
            );

        if (clientSecret is not null)
        {
            keycloakContainer.WithEnvironment($"CLIENT_{clientEnv}_SECRET", clientSecret);
        }

        keycloakContainer.OnResourceEndpointsAllocated(
            (resource, _, _) =>
            {
                var resourceBuilder = clientBuilder.ApplicationBuilder.CreateResourceBuilder(
                    resource
                );

                resourceBuilder.WithEnvironment(context =>
                {
                    var endpoint = clientBuilder.GetEndpoint(Http.Schemes.Http);

                    context.EnvironmentVariables.Add($"CLIENT_{clientEnv}_URL", endpoint.Url);

                    if (includeContainerHostUrl)
                    {
                        context.EnvironmentVariables.Add(
                            $"CLIENT_{clientEnv}_URL_CONTAINERHOST",
                            endpoint
                        );
                    }
                });

                return Task.CompletedTask;
            }
        );
    }

    private static void ConfigureClientForHostedKeycloak<TResource>(
        IResourceBuilder<TResource> clientBuilder,
        IResourceBuilder<ExternalServiceResource> keycloakHosted,
        IResourceBuilder<ParameterResource> betterAuthSecret,
        string clientId
    )
        where TResource : IResourceWithEnvironment, IResourceWithWaitSupport
    {
        var realmParameter = clientBuilder
            .ApplicationBuilder.Resources.OfType<ParameterResource>()
            .First(r => string.Equals(r.Name, "kc-realm", StringComparison.OrdinalIgnoreCase));

        clientBuilder
            .WithReference(keycloakHosted)
            .WaitFor(keycloakHosted)
            .WithEnvironment("BETTER_AUTH_SECRET", betterAuthSecret)
            .WithEnvironment("KEYCLOAK_URL", keycloakHosted)
            .WithEnvironment("KEYCLOAK_REALM", realmParameter)
            .WithEnvironment("KEYCLOAK_CLIENT_ID", clientId);
    }

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
                .WithOtlpExporter()
                .WithDataVolume()
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
                .WithEnvironment(RealmDisplayName, displayName);

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
