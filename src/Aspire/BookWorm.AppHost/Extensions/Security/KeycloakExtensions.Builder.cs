namespace BookWorm.AppHost.Extensions.Security;

internal static partial class KeycloakExtensions
{
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
                .WithDataVolume()
                .WithOtlpExporter()
                .WithArgs("--health-enabled=true")
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
                        Description = "Enter your Keycloak server URL here (must be https)",
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
