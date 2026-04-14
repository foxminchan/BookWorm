namespace BookWorm.AppHost.Extensions.Security;

internal static partial class KeycloakExtensions
{
    extension(IResourceBuilder<ProjectResource> builder)
    {
        /// <summary>
        ///     Configures the project resource to integrate with Keycloak as an Identity Provider (IdP).
        /// </summary>
        /// <param name="keycloak">The Keycloak resource builder to configure as an IdP.</param>
        /// <returns>The project resource builder for method chaining.</returns>
        public IResourceBuilder<ProjectResource> WithKeycloak(IResourceBuilder<IResource> keycloak)
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
                        "API",
                        clientSecret,
                        true
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
                                Name = $"KeycloakClientSecretParameter-{clientId}",
                                Label = $"Keycloak Client Secret ({clientId})",
                                InputType = InputType.SecretText,
                                Description = $"Enter the Keycloak client secret for {clientId}",
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
    }
}
