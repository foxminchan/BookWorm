using System.Security.Claims;

namespace BookWorm.Ordering.Infrastructure.Identity;

public class IdentityService(IHttpContextAccessor httpContext) : IIdentityService
{
    public string? GetUserIdentity()
    {
        return httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? GetUserName()
    {
        return httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
    }
}
