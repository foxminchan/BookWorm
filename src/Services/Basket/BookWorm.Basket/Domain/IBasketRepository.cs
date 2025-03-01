namespace BookWorm.Basket.Domain;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(string id);
    Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync(string id);
}
