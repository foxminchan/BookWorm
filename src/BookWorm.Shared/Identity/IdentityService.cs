using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Shared.Identity;

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

    public string? GetEmail()
    {
        return httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    }

    public bool IsAdminRole()
    {
        return httpContext.HttpContext?.User.IsInRole("Admin") ?? false;
    }
}
