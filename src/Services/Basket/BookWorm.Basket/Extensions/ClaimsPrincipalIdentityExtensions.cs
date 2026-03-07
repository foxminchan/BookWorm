using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;

namespace BookWorm.Basket.Extensions;

internal static class ClaimsPrincipalIdentityExtensions
{
    public static string GetAuthenticatedUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var sub = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);
        return Guard.Against.NotAuthenticated(sub);
    }
}
