using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace BookWorm.Chassis.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        public string[] GetRoles()
        {
            return [.. claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value)];
        }

        public string? GetClaimValue(string claimType)
        {
            var claim = claimsPrincipal.FindFirst(claimType);
            return claim?.Value;
        }

        public bool TryGetJsonClaim(string claimType, [NotNullWhen(true)] out JsonNode? claimJson)
        {
            var candidateClaim = claimsPrincipal.FindFirst(claimType);

            claimJson =
                candidateClaim is not null
                && string.Equals(
                    candidateClaim.ValueType,
                    "JSON",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? JsonNode.Parse(candidateClaim.Value)
                    : null;

            return claimJson is not null;
        }
    }
}
