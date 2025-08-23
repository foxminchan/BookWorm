using BookWorm.Basket.Infrastructure.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Basket.Domain;

[method: JsonConstructor]
public sealed class CustomerBasket() : AuditableEntity<string>
{
    private readonly List<BasketItem> _basketItems = [];

    public CustomerBasket(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        List<BasketItem> items
    )
        : this()
    {
        Id = id ?? throw new BasketDomainException("Customer ID cannot be null.");
        _basketItems =
            items.Count > 0
                ? items
                : throw new BasketDomainException("Basket must contain at least one item.");
    }

    public IReadOnlyCollection<BasketItem> Items => _basketItems.AsReadOnly();

    /// <summary>
    ///     Updates the customer basket by replacing all existing items with the provided items.
    /// </summary>
    /// <param name="requestItems">The list of basket items to replace the current items with.</param>
    /// <returns>The current instance of <see cref="CustomerBasket" /> with updated items.</returns>
    public CustomerBasket Update(List<BasketItem> requestItems)
    {
        _basketItems.Clear();
        _basketItems.AddRange(requestItems);
        return this;
    }
}
