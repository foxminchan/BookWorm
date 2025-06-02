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
                .WithRealmImport("Container/realms", true)
                .WithEnvironment("REALM_NAME", realmName)
                .WithEnvironment("REALM_DISPLAY_NAME", displayName);
        }

        return builder;
    }

    /// <summary>
    ///     Configures a project resource with Keycloak integration.
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="keycloak">The Keycloak resource builder to reference.</param>
    /// <returns>The project resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method configures the project to reference the Keycloak resource,
    ///     sets up the identity URL environment variable, and ensures the project
    ///     waits for Keycloak to be ready before starting.
    /// </remarks>
    public static IResourceBuilder<ProjectResource> WithKeycloak(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<KeycloakResource> keycloak
    )
    {
        return builder
            .WithReference(keycloak)
            .WithEnvironment("Identity__Url", keycloak.GetEndpoint("http"))
            .WaitFor(keycloak);
    }
}
