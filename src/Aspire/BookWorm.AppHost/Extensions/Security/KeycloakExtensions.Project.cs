using Aspire.Hosting.Dotnet;

namespace BookWorm.AppHost.Extensions.Security;

internal static partial class KeycloakExtensions
{
    extension<TResource>(IResourceBuilder<TResource> builder)
        where TResource : IResourceWithEnvironment, IResourceWithWaitSupport
    {
        /// <summary>
        ///     Configures a resource to use the supplied Keycloak realm. Project resources also
        ///     receive their Keycloak client configuration.
        /// </summary>
        /// <param name="keycloak">The Keycloak resource builder to configure as an identity provider.</param>
        /// <param name="audienceClientId">
        ///     Optional audience-only client to add to the local Keycloak realm import.
        /// </param>
        /// <returns>The resource builder for method chaining.</returns>
        public IResourceBuilder<TResource> WithKeycloak(
            IResourceBuilder<IResource> keycloak,
            string? audienceClientId = null
        )
        {
            var clientId = builder.Resource.Name;
            var applicationBuilder = builder.ApplicationBuilder;

            switch (keycloak)
            {
                case IResourceBuilder<KeycloakResource> keycloakContainer:
                {
                    if (!string.IsNullOrWhiteSpace(audienceClientId))
                    {
                        keycloakContainer.WithAudienceClient(audienceClientId, "API");
                    }

                    builder
                        .WithReference(keycloakContainer)
                        .WaitForStart(keycloakContainer)
                        .WithEnvironment("Identity__Realm", _defaultLocalKeycloakName);

                    if (builder is IResourceBuilder<DotnetProjectResource> dotnetProject)
                    {
                        var clientSecret = applicationBuilder
                            .AddParameter($"{clientId}-secret", true)
                            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

                        ConfigureKeycloakForClient(
                            keycloakContainer,
                            dotnetProject,
                            clientId,
                            "API",
                            clientSecret,
                            true
                        );

                        dotnetProject
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
                    }

                    break;
                }
                case IResourceBuilder<ExternalServiceResource> keycloakHosted:
                {
                    var realmParameter = applicationBuilder
                        .Resources.OfType<ParameterResource>()
                        .First(resource =>
                            string.Equals(
                                resource.Name,
                                "kc-realm",
                                StringComparison.OrdinalIgnoreCase
                            )
                        );

                    builder
                        .WithReference(keycloakHosted)
                        .WaitFor(keycloakHosted)
                        .WithEnvironment("Identity__Realm", realmParameter);

                    if (builder is IResourceBuilder<DotnetProjectResource> dotnetProject)
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
                                    Description =
                                        $"Enter the Keycloak client secret for {clientId}",
                                }
                            );

                        dotnetProject
                            .WithEnvironment("Identity__ClientId", clientId)
                            .WithEnvironment("Identity__ClientSecret", clientSecret)
                            .WithEnvironment(
                                $"Identity__Scopes__{clientId}",
                                Services.ToClientName(clientId, "API")
                            );
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException(
                        $"Unsupported Keycloak resource type: {keycloak.Resource.GetType().Name}"
                    );
            }

            return builder;
        }
    }
}
