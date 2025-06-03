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
    ///     Configures a ProjectResource with Keycloak integration.
    /// </summary>
    /// <param name="builder">The ProjectResource builder to configure.</param>
    /// <param name="keycloak">The Keycloak resource builder to reference.</param>
    /// <returns>The ProjectResource builder for method chaining.</returns>
    /// <remarks>
    ///     This extension adds a reference to the Keycloak resource and sets the Identity URL
    ///     environment variable when running in execution mode.
    /// </remarks>
    public static IResourceBuilder<ProjectResource> WithKeycloak(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<KeycloakResource> keycloak
    )
    {
        builder.WithReference(keycloak).WaitFor(keycloak);

        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.WithEnvironment("Identity__Url", keycloak.GetEndpoint("http"));
        }

        return builder;
    }
}
