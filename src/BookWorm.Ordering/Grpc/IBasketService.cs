namespace BookWorm.Ordering.Grpc;

public interface IBasketService
{
    Task<Basket> GetBasketAsync(Guid basketId, CancellationToken cancellationToken = default);
}
