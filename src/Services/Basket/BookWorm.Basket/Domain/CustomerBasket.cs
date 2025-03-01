namespace BookWorm.Basket.Domain;

[method: JsonConstructor]
public sealed class CustomerBasket() : IAggregateRoot
{
    private readonly List<BasketItem> _basketItems = [];

    public CustomerBasket(string id, List<BasketItem> items)
        : this()
    {
        Id = id;
        _basketItems = items;
    }

    public string? Id { get; private set; }

    public IReadOnlyCollection<BasketItem> Items => _basketItems.AsReadOnly();

    public void Update(List<BasketItem> requestItems)
    {
        _basketItems.Clear();
        _basketItems.AddRange(requestItems);
    }
}
