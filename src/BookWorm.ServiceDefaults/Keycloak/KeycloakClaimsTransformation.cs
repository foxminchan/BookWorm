using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

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

        if (!principal.TryGetJsonClaim("resource_access", out var resourceAccess))
        {
            return Task.FromResult(principal);
        }

        if (!principal.TryGetJsonClaim("realm_access", out var realmAccess))
        {
            return Task.FromResult(principal);
        }

        if (
            resourceAccess[clientId] is not JsonObject resourceNode
            || resourceNode["roles"] is not JsonArray resourceRoles
        )
        {
            return Task.FromResult(principal);
        }

        if (realmAccess["roles"] is not JsonArray realmRoles)
        {
            return Task.FromResult(principal);
        }

        var claimsIdentity = new ClaimsIdentity();

        // Convert resource roles to regular roles.
        foreach (var role in resourceRoles.GetValues<string>())
        {
            if (!claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                claimsIdentity.AddClaim(new(ClaimTypes.Role, role));
            }
        }

        // Convert realm roles to regular roles.
        foreach (var role in realmRoles.GetValues<string>())
        {
            if (!claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                claimsIdentity.AddClaim(new(ClaimTypes.Role, role));
            }
        }

        principal.AddIdentity(claimsIdentity);

        return Task.FromResult(principal);
    }
}
