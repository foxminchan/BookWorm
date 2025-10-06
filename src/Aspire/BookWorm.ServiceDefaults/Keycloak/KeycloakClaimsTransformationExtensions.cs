namespace BookWorm.ServiceDefaults.Keycloak;

public static class KeycloakClaimsTransformationExtensions
{
    /// <summary>
    ///     Adds an <see cref="IClaimsTransformation" /> that transforms Keycloak resource access roles claims into regular
    ///     role claims.
    /// </summary>
    public static void WithKeycloakClaimsTransformation(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
    }
}
