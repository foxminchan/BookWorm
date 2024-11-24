namespace BookWorm.Catalog.UnitTests.Builder;

public static class PublisherBuilder
{
    private static List<Publisher> _publishers = [];

    public static List<Publisher> WithDefaultValues()
    {
        _publishers =
        [
            new("Addison-Wesley"),
            new("Manning"),
            new("O'Reilly"),
            new("Pragmatic Bookshelf"),
            new("Packt"),
        ];

        return _publishers;
    }
}
