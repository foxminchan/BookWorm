using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;

namespace BookWorm.Basket.Extensions;

internal static class ClaimsPrincipalIdentityExtensions
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        public string GetAuthenticatedUserId()
        {
            var sub = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);
            return Guard.Against.NotAuthenticated(sub);
        }
    }
}
