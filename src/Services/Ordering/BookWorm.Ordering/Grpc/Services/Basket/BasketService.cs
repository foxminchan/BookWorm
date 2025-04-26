using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;

namespace BookWorm.Ordering.Grpc.Services.Basket;

[ExcludeFromCodeCoverage]
public sealed class BasketService(BasketGrpcServiceClient service) : IBasketService
{
    public async Task<BasketResponse> GetBasket(CancellationToken cancellationToken = default)
    {
        var result = await service.GetBasketAsync(new(), cancellationToken: cancellationToken);

        return result;
    }
}
