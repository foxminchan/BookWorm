namespace BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;

[ExcludeFromCodeCoverage]
public sealed class CategoryData : List<Category>
{
    public CategoryData()
    {
        AddRange(
            [
                new("Fiction"),
                new("Non-Fiction"),
                new("Science Fiction"),
                new("Fantasy"),
                new("Mystery"),
                new("Biography"),
                new("History"),
                new("Romance"),
                new("Self-Help"),
                new("Children's Books"),
            ]
        );
    }
}
