using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.UnitTests.Builder;

public static class OrderBuilder
{
    private static List<Order> _orders = [];

    public static List<Order> WithDefaultValues()
    {
        _orders =
        [
            new(Guid.NewGuid(), "Note 1"),
            new(Guid.NewGuid(), "Note 2"),
            new(Guid.NewGuid(), "Note 3"),
            new(Guid.NewGuid(), "Note 4"),
            new(Guid.NewGuid(), "Note 5")
        ];

        return _orders;
    }
}
