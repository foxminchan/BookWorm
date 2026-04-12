using BookWorm.Chassis.Security.Extensions;
using Grpc.Core;

namespace BookWorm.Basket.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    extension(ServerCallContext context)
    {
        public string? GetUserIdentity()
        {
            return context.GetHttpContext().User.GetClaimValue(ClaimTypes.NameIdentifier);
        }
    }
}
