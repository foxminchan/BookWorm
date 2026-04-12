using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace BookWorm.Chassis.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        /// <summary>
        ///     Gets all role claim values from the current principal.
        /// </summary>
        /// <returns>
        ///     An array containing the role values.
        /// </returns>
        public string[] GetRoles()
        {
            return [.. claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value)];
        }

        /// <summary>
        ///     Determines whether the current principal has the specified role.
        /// </summary>
        /// <param name="role">
        ///     The role value to match.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a matching role claim exists; otherwise, <see langword="false" />.
        /// </returns>
        public bool HasRole(string role)
        {
            return claimsPrincipal.FindAll(ClaimTypes.Role).Any(c => c.Value == role);
        }

        /// <summary>
        ///     Gets the value of the first claim that matches the provided claim type.
        /// </summary>
        /// <param name="claimType">
        ///     The claim type to search for.
        /// </param>
        /// <returns>
        ///     The claim value when a matching claim exists; otherwise, <see langword="null" />.
        /// </returns>
        public string? GetClaimValue(string claimType)
        {
            var claim = claimsPrincipal.FindFirst(claimType);
            return claim?.Value;
        }

        /// <summary>
        ///     Attempts to read a JSON-typed claim and parse it as a <see cref="JsonNode" />.
        /// </summary>
        /// <param name="claimType">
        ///     The claim type to search for.
        /// </param>
        /// <param name="claimJson">
        ///     When this method returns, contains the parsed JSON value when the claim exists and is valid JSON; otherwise,
        ///     <see langword="null" />. This parameter is treated as uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a JSON claim was found and parsed successfully; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="System.Text.Json.JsonException">
        ///     The claim value has a JSON value type marker but contains invalid JSON content.
        /// </exception>
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
