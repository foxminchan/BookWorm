using BookWorm.Basket.Exceptions;

namespace BookWorm.Basket.Domain;

[method: JsonConstructor]
public sealed class CustomerBasket()
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

    public string? Id { get; private set; }

    public IReadOnlyCollection<BasketItem> Items => _basketItems.AsReadOnly();

    public void Update(List<BasketItem> requestItems)
    {
        _basketItems.Clear();
        _basketItems.AddRange(requestItems);
    }
}
