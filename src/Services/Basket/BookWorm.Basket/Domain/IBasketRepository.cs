namespace BookWorm.Basket.Domain;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id
    );

    Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync([StringSyntax(StringSyntaxAttribute.GuidFormat)] string id);
}
