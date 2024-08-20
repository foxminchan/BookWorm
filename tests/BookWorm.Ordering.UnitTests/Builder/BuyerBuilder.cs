using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.UnitTests.Builder;

public static class BuyerBuilder
{
    private static List<Buyer> _buyers = [];

    public static List<Buyer> WithDefaultValues()
    {
        _buyers =
        [
            new(Guid.NewGuid(), "John Doe", new("123 Main St", "City", "Province")),
            new(Guid.NewGuid(), "Jane Doe", new("456 Main St", "City", "Province")),
            new(Guid.NewGuid(), "Alice Doe", new("789 Main St", "City", "Province")),
            new(Guid.NewGuid(), "Bob Doe", new("101 Main St", "City", "Province")),
            new(Guid.NewGuid(), "Charlie Doe", new("202 Main St", "City", "Province"))
        ];

        return _buyers;
    }
}
