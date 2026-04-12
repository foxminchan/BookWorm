namespace BookWorm.Basket.Domain;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id
    );

    Task<CustomerBasket?> CreateOrUpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync([StringSyntax(StringSyntaxAttribute.GuidFormat)] string id);
}
