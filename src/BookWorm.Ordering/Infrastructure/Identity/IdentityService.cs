using System.Security.Claims;

namespace BookWorm.Ordering.Infrastructure.Identity;

public class IdentityService(IHttpContextAccessor httpContext) : IIdentityService
{
    public string? GetUserIdentity()
    {
        return httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? GetFullName()
    {
        return httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
    }

    public bool IsAdminRole()
    {
        return httpContext.HttpContext?.User.IsInRole("Admin") ?? false;
    }
}
