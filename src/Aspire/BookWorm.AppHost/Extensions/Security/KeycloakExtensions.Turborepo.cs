namespace BookWorm.AppHost.Extensions.Security;

internal static partial class KeycloakExtensions
{
    extension(IResourceBuilder<TurborepoAppResource> builder)
    {
        /// <summary>
        ///     Configures the turborepo app resource to integrate with Keycloak as an Identity Provider (IdP).
        /// </summary>
        /// <param name="keycloak">The Keycloak resource builder to configure as an IdP.</param>
        /// <returns>The turborepo app resource builder for method chaining.</returns>
        public IResourceBuilder<TurborepoAppResource> WithKeycloak(
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
                        "APP",
                        null,
                        false
                    );

                    builder
                        .WithReference(keycloakContainer)
                        .WaitForStart(keycloakContainer)
                        .WithEnvironment("BETTER_AUTH_SECRET", betterAuthSecret)
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
    }
}
