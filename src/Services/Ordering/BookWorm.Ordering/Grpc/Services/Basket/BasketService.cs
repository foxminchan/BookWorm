namespace BookWorm.Ordering.Grpc.Services.Basket;

[ExcludeFromCodeCoverage]
internal sealed class BasketService(BasketGrpcService.BasketGrpcServiceClient service)
    : IBasketService
{
    public async Task<BasketResponse> GetBasket(CancellationToken cancellationToken = default)
    {
        var result = await service.GetBasketAsync(new(), cancellationToken: cancellationToken);

        return result;
    }
}
