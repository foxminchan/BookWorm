namespace BookWorm.Ordering.Grpc.Services.Basket;

public interface IBasketService
{
    Task<BasketResponse> GetBasket(CancellationToken cancellationToken = default);
}
