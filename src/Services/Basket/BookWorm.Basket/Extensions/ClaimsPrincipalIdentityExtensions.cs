using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;

namespace BookWorm.Basket.Extensions;

internal static class ClaimsPrincipalIdentityExtensions
{
    /// <summary>
    ///     Extracts and validates the authenticated user's ID from the claims principal.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract the user ID from.</param>
    /// <returns>The validated user ID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    public static string GetAuthenticatedUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var sub = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);
        return Guard.Against.NotAuthenticated(sub);
    }
}
