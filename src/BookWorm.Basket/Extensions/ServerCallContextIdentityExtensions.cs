using System.Security.Claims;

namespace BookWorm.Basket.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context)
    {
        return context.GetHttpContext().User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetUserName(this ServerCallContext context)
    {
        return context.GetHttpContext().User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
    }
}
