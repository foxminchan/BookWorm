using BookWorm.Chassis.Logging;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace BookWorm.Basket.Grpc.Services.Basket;

internal sealed partial class BasketService(
    IBasketRepository repository,
    ILogger<BasketService> logger
) : BasketGrpcService.BasketGrpcServiceBase
{
    [Authorize]
    [EnableRateLimiting("PerUserRateLimit")]
    public override async Task<GetBasketResponse> GetBasket(
        Empty request,
        ServerCallContext context
    )
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

        LogGetBasketById(logger, context.Method, userId);

        var data = await repository.GetBasketAsync(userId);

        return data is not null ? MapToBasketResponse(data) : new();
    }

    [LoggerMessage(
        EventId = 0,
        EventName = nameof(LogGetBasketById),
        Level = LogLevel.Debug,
        Message = "Begin GetBasketById call from method {Method} for basket id {Id}"
    )]
    private static partial void LogGetBasketById(
        ILogger logger,
        string method,
        [PIIData] string id
    );

    private static GetBasketResponse MapToBasketResponse(CustomerBasket basket)
    {
        var response = new GetBasketResponse { Id = (string)basket.Id };
        var items = basket.Items.Select(item => new Item
        {
            Id = item.Id,
            Quantity = item.Quantity,
        });
        response.Items.AddRange(items);
        return response;
    }
}
