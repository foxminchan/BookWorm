namespace BookWorm.Basket.Domain;

public sealed class BasketItem(Guid id, int quantity)
{
    public Guid Id { get; private set; } = id;
    public int Quantity { get; private set; } = quantity;

    public void IncreaseQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void ReduceQuantity(int quantity)
    {
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
