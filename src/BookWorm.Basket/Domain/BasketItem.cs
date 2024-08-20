namespace BookWorm.Basket.Domain;

public sealed class BasketItem(Guid id, int quantity)
{
    public Guid Id { get; private set; } = id;
    public int Quantity { get; private set; } = Guard.Against.OutOfRange(quantity, nameof(quantity), 1, int.MaxValue);

    public void IncreaseQuantity(int quantity)
    {
        Quantity += Guard.Against.OutOfRange(quantity, nameof(quantity), 1, int.MaxValue);
    }

    public void ReduceQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        if (Quantity - quantity < 0)
        {
            Quantity = 0;
        }
        else
        {
            Quantity -= quantity;
        }
    }
}
