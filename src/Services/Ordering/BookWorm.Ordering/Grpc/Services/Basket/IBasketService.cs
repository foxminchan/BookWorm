namespace BookWorm.Ordering.Grpc.Services.Basket;

public interface IBasketService
{
    Task<GetBasketResponse> GetBasket(CancellationToken cancellationToken = default);
}
