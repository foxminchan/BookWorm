using BookWorm.Basket.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace BookWorm.Basket.Grpc.Services.Basket;

public sealed class BasketService(IBasketRepository repository, ILogger<BasketService> logger)
    : BasketGrpcService.BasketGrpcServiceBase
{
    [Authorize]
    [EnableRateLimiting("PerUserRateLimit")]
    public override async Task<BasketResponse> GetBasket(Empty request, ServerCallContext context)
    {
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning(
                "User identity is null or empty for method {Method} in {Service}",
                context.Method,
                nameof(BasketService)
            );
            throw new RpcException(new(StatusCode.Unauthenticated, "User is not authenticated."));
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Begin GetBasketById call from method {Method} for basket id {Id}",
                context.Method,
                userId
            );
        }

        var data = await repository.GetBasketAsync(userId);

        return data is not null ? MapToBasketResponse(data) : new();
    }

    private static BasketResponse MapToBasketResponse(CustomerBasket basket)
    {
        var response = new BasketResponse { Id = basket.Id };
        var items = basket.Items.Select(item => new Item
        {
            Id = item.Id,
            Quantity = item.Quantity,
        });
        response.Items.AddRange(items);
        return response;
    }
}
