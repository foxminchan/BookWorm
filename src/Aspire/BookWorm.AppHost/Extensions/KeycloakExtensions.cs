namespace BookWorm.AppHost.Extensions;

public static class KeycloakExtensions
{
    private const string BaseContainerPath = "Container/keycloak";

    /// <summary>
    ///     Configures a Keycloak resource with a sample realm import.
    /// </summary>
    /// <param name="builder">The Keycloak resource builder.</param>
    /// <param name="realmName">The name of the realm to import.</param>
    /// <param name="displayName">The display name of the realm.</param>
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This extension only imports the realm when running in execution mode,
    ///     not during build or design time.
    /// </remarks>
    public static IResourceBuilder<KeycloakResource> WithSampleRealmImport(
        this IResourceBuilder<KeycloakResource> builder,
        string realmName,
        string displayName
    )
    {
        builder
            .WithRealmImport($"{BaseContainerPath}/realms", true)
            .WithEnvironment("REALM_NAME", realmName)
            .WithEnvironment("REALM_DISPLAY_NAME", displayName)
            // Ensure HSTS is not enabled in run mode to avoid browser caching issues when developing.
            // Workaround for https://github.com/keycloak/keycloak/issues/32366
            .WithEnvironment(
                "REALM_HSTS",
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
    /// <returns>The Keycloak resource builder for method chaining.</returns>
    /// <remarks>
    ///     This extension mounts the custom themes directory from the host machine
    ///     to the Keycloak container's providers directory.
    ///     The mount is only performed if the themes directory exists on the host machine.
    /// </remarks>
    public static IResourceBuilder<KeycloakResource> WithCustomTheme(
        this IResourceBuilder<KeycloakResource> builder
    )
    {
        var importFullPath = Path.GetFullPath(
            $"{BaseContainerPath}/themes",
            builder.ApplicationBuilder.AppHostDirectory
        );

        if (Directory.Exists(importFullPath))
        {
            builder.WithBindMount(importFullPath, "/opt/keycloak/providers/", true);
        }

        return builder;
    }
}
