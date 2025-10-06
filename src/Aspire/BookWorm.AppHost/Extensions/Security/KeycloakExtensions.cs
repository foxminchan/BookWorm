using BookWorm.Constants.Core;

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
    ///     Configures a <see cref="KeycloakResource" /> to use an external PostgreSQL database.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{KeycloakResource}" /> instance.</param>
    /// <param name="pgDatabase">The PostgreSQL database resource builder.</param>
    /// <param name="xaEnabled">Whether to enable XA transactions. Default is false.</param>
    /// <returns>The <see cref="IResourceBuilder{KeycloakResource}" /> for method chaining.</returns>
    public static IResourceBuilder<KeycloakResource> WithPostgres(
        this IResourceBuilder<KeycloakResource> builder,
        IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> pgDatabase,
        bool xaEnabled = false
    )
    {
        return builder
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add(KeycloakDatabaseEnvVarName, "postgres");
                context.EnvironmentVariables.Add(
                    KeycloakDatabaseUsernameEnvVarName,
                    pgDatabase.Resource.Parent.UserName ?? ReferenceExpression.Create($"postgres")
                );
                context.EnvironmentVariables.Add(
                    KeycloakDatabasePasswordEnvVarName,
                    pgDatabase.Resource.Parent.Password ?? ReferenceExpression.Create($"postgres")
                );
                context.EnvironmentVariables.Add(
                    KeycloakDatabaseUrlEnvVarName,
                    ReferenceExpression.Create(
                        $"jdbc:postgresql://{pgDatabase.Resource.Parent.HostName}/{pgDatabase.Resource.DatabaseName}"
                    )
                );
                context.EnvironmentVariables.Add(
                    KeycloakTransactionXaEnabledEnvVarName,
                    xaEnabled.ToString().ToLowerInvariant()
                );
            })
            .WaitFor(pgDatabase);
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
        IResourceBuilder<IResource> keycloak,
        IResourceBuilder<ParameterResource> realmName
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

                            context.EnvironmentVariables[$"CLIENT_{clientEnv}_URL"] = context
                                .ExecutionContext
                                .IsPublishMode
                                ? endpoint
                                : endpoint.Url;

                            context.EnvironmentVariables[$"CLIENT_{clientEnv}_URL_CONTAINERHOST"] =
                                endpoint;
                        });

                        return Task.CompletedTask;
                    }
                );

            builder
                .WithReference(keycloakContainer)
                .WaitForStart(keycloakContainer)
                .WithEnvironment("Identity__Realm", realmName)
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

            builder
                .WithReference(keycloakHosted)
                .WaitFor(keycloakHosted)
                .WithEnvironment("Identity__Realm", realmName)
                .WithEnvironment("Identity__ClientId", clientId)
                .WithEnvironment("Identity__ClientSecret", clientSecret)
                .WithEnvironment($"Identity__Scopes__{clientId}", clientId.ToClientName("API"));
        }

        return builder;
    }
}
