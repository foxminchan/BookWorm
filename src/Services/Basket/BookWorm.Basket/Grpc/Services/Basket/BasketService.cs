using BookWorm.Basket.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Basket.Grpc.Services.Basket;

public sealed class BasketService(IBasketRepository repository, ILogger<BasketService> logger)
    : BasketGrpcService.BasketGrpcServiceBase
{
    [AllowAnonymous]
    public override async Task<BasketResponse> GetBasket(Empty request, ServerCallContext context)
    {
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            return new();
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
