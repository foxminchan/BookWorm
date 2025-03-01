using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;

namespace BookWorm.Ordering.Grpc.Services.Basket;

public sealed class BasketService(BasketGrpcServiceClient service, ILogger<BookService> logger)
    : IBasketService
{
    public async Task<BasketResponse> GetBasket(CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Begin grpc call {Method}",
                nameof(BasketService),
                nameof(GetBasket)
            );
        }

        var result = await service.GetBasketAsync(new(), cancellationToken: cancellationToken);

        return result;
    }
}
