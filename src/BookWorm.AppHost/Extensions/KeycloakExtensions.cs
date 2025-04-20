namespace BookWorm.AppHost.Extensions;

public static class KeycloakExtensions
{
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
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder
                .WithRealmImport("realms", true)
                .WithEnvironment("REALM_NAME", realmName)
                .WithEnvironment("REALM_DISPLAY_NAME", displayName);
        }

        return builder;
    }
}
