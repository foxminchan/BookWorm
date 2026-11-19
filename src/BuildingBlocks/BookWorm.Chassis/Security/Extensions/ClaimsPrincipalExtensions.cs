using System.Security.Claims;

namespace BookWorm.Chassis.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        /// <summary>
        ///     Gets the value of the first claim that matches the provided claim type.
        /// </summary>
        /// <param name="claimType">
        ///     The claim type to search for.
        /// </param>
        /// <returns>
        ///     The claim value when a matching claim exists; otherwise, <see langword="null" />.
        /// </returns>
        public string? this[string claimType] => claimsPrincipal.FindFirst(claimType)?.Value;

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
    }
}
