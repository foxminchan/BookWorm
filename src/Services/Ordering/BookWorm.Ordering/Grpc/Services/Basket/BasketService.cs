namespace BookWorm.Ordering.Grpc.Services.Basket;

[ExcludeFromCodeCoverage]
internal sealed class BasketService(BasketGrpcService.BasketGrpcServiceClient service)
    : IBasketService
{
    public async Task<GetBasketResponse> GetBasket(CancellationToken cancellationToken = default)
    {
        var result = await service.GetBasketAsync(
            new(),
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: cancellationToken
        );

        return result;
    }
}
