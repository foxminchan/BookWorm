using Grpc.Core;

namespace BookWorm.Basket.Extensions;

public static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context)
    {
        return context.GetHttpContext().User.GetClaimValue(ClaimTypes.NameIdentifier);
    }
}
