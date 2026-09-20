namespace BookWorm.AppHost.Extensions.Security;

internal static partial class KeycloakExtensions
{
    public static IResourceBuilder<ContainerResource> ConfigureContainerKeycloak(
        IResourceBuilder<ContainerResource> builder,
        IResourceBuilder<IResource> keycloak
    ) => WithKeycloak(builder, keycloak);

    extension(IResourceBuilder<ContainerResource> builder)
    {
        public IResourceBuilder<ContainerResource> WithKeycloak(
            IResourceBuilder<IResource> keycloak
        )
        {
            var clientId = builder.Resource.Name;
            var applicationBuilder = builder.ApplicationBuilder;

            switch (keycloak)
            {
                case IResourceBuilder<KeycloakResource> keycloakContainer:
                {
                    var betterAuthSecret = applicationBuilder
                        .AddParameter($"{clientId}-better-auth-secret", true)
                        .WithGeneratedDefault(new() { MinLength = 32, Special = false });

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
                }
                case IResourceBuilder<ExternalServiceResource> keycloakHosted:
                    ConfigureClientForHostedKeycloak(
                        builder,
                        keycloakHosted,
                        applicationBuilder
                            .AddParameter($"{clientId}-better-auth-secret", true)
                            .WithGeneratedDefault(new() { MinLength = 32, Special = false }),
                        clientId
                    );
                    break;
            }

            return builder;
        }
    }
}
