using BookWorm.Basket.Infrastructure.Exceptions;
using BookWorm.SharedKernel.SeedWork.Model;

namespace BookWorm.Basket.Domain;

[method: JsonConstructor]
public sealed class CustomerBasket() : AuditableEntity<string>
{
    private readonly List<BasketItem> _basketItems = [];

    public CustomerBasket(string id, List<BasketItem> items)
        : this()
    {
        Id = id ?? throw new BasketDomainException("Customer ID cannot be null.");
        _basketItems =
            items.Count > 0
                ? items
                : throw new BasketDomainException("Basket must contain at least one item.");
    }

    public IReadOnlyCollection<BasketItem> Items => _basketItems.AsReadOnly();

    public void Update(List<BasketItem> requestItems)
    {
        _basketItems.Clear();
        _basketItems.AddRange(requestItems);
    }
}
