namespace BookWorm.Basket.Domain;

public sealed class Basket(Guid accountId, List<BasketItem> basketItems)
{
    public Guid AccountId { get; private set; } = accountId;
    public ICollection<BasketItem> BasketItems { get; private set; } = basketItems;

    public void AddItem(BasketItem item)
    {
        var existingItem = BasketItems.FirstOrDefault(x => x.Id == item.Id);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(item.Quantity);
        }
        else
        {
            BasketItems.Add(item);
        }
    }

    public void RemoveItem(Guid itemId)
    {
        var item = BasketItems.FirstOrDefault(x => x.Id == itemId);

        if (item is not null)
        {
            BasketItems.Remove(item);
        }
    }

    public void ReduceItemQuantity(Guid itemId, int quantity)
    {
        var item = BasketItems.FirstOrDefault(x => x.Id == itemId);

        if (item is null)
        {
            return;
        }

        item.ReduceQuantity(quantity);

        if (item.Quantity <= 0)
        {
            BasketItems.Remove(item);
        }
    }
}
