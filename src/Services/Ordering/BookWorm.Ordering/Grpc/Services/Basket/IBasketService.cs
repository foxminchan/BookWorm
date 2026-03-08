namespace BookWorm.Ordering.Grpc.Services.Basket;

internal interface IBasketService
{
    Task<GetBasketResponse> GetBasket(CancellationToken cancellationToken = default);
}
