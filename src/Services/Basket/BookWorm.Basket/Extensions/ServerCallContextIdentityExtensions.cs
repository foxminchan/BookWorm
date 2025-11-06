using BookWorm.Chassis.Security.Extensions;
using Grpc.Core;

namespace BookWorm.Basket.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context)
    {
        return context.GetHttpContext().User.GetClaimValue(ClaimTypes.NameIdentifier);
    }
}
