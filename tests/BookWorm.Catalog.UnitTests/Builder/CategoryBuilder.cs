namespace BookWorm.Catalog.UnitTests.Builder;

public static class CategoryBuilder
{
    private static List<Category> _categories = [];

    public static List<Category> WithDefaultValues()
    {
        _categories =
        [
            new("Software Development"),
            new("Design Patterns"),
            new("Clean Code"),
            new("Refactoring"),
            new("Agile"),
            new("Software Architecture")
        ];

        return _categories;
    }
}
