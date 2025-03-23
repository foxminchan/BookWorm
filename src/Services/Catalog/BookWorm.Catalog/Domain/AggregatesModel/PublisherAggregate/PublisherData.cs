namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

[ExcludeFromCodeCoverage]
public sealed class PublisherData : List<Publisher>
{
    public PublisherData()
    {
        AddRange(
            [
                new("Penguin Random House"),
                new("HarperCollins"),
                new("Simon & Schuster"),
                new("Hachette Book Group"),
                new("Macmillan Publishers"),
                new("Scholastic Inc."),
                new("Wiley"),
                new("Springer Nature"),
                new("Oxford University Press"),
                new("Cambridge University Press"),
            ]
        );
    }
}
