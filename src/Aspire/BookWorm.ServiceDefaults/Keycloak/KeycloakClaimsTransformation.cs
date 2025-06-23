namespace BookWorm.ServiceDefaults.Keycloak;

/// <summary>
///     Transforms Keycloak resource roles claims into regular role claims.
/// </summary>
/// <remarks>
///     Learn more about claims transformation in ASP.NET Core at
///     <see
///         href="https://learn.microsoft.com/aspnet/core/security/authentication/claims#extend-or-add-custom-claims-using-iclaimstransformation">
///         https://learn.microsoft.com/aspnet/core/security/authentication/claims#extend-or-add-custom-claims-using-iclaimstransformation
///     </see>
/// </remarks>
public sealed class KeycloakRolesClaimsTransformation(
    IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions
) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var options = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        var clientId =
            options.TokenValidationParameters.ValidAudience
            ?? options.TokenValidationParameters.ValidAudiences.FirstOrDefault()
            ?? throw new InvalidOperationException("Audience is not set on JwtBearerOptions");

        if (
            !principal.TryGetJsonClaim("resource_access", out var resourceAccess)
            || !principal.TryGetJsonClaim("realm_access", out var realmAccess)
            || resourceAccess[clientId] is not JsonObject resourceNode
            || resourceNode["roles"] is not JsonArray resourceRoles
            || realmAccess["roles"] is not JsonArray realmRoles
        )
        {
            return Task.FromResult(principal);
        }

        var claimsIdentity = new ClaimsIdentity();

        // Convert resource roles to regular roles.
        var resourceRoleClaims = resourceRoles
            .GetValues<string>()
            .Where(role =>
                !claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role)
            )
            .Select(role => new Claim(ClaimTypes.Role, role));

        claimsIdentity.AddClaims(resourceRoleClaims);

        // Convert realm roles to regular roles.
        var realmRoleClaims = realmRoles
            .GetValues<string>()
            .Where(role =>
                !claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role)
            )
            .Select(role => new Claim(ClaimTypes.Role, role));

        claimsIdentity.AddClaims(realmRoleClaims);

        principal.AddIdentity(claimsIdentity);

        return Task.FromResult(principal);
    }
}
