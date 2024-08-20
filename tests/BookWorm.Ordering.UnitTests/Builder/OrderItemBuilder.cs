using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.UnitTests.Builder;

public static class OrderItemBuilder
{
    private static List<OrderItem> _orderItems = [];

    public static List<OrderItem> WithDefaultValues()
    {
        _orderItems =
        [
            new(Guid.NewGuid(), 1, 10.00m),
            new(Guid.NewGuid(), 2, 20.00m),
            new(Guid.NewGuid(), 3, 30.00m),
            new(Guid.NewGuid(), 4, 40.00m),
            new(Guid.NewGuid(), 5, 50.00m)
        ];

        return _orderItems;
    }
}
