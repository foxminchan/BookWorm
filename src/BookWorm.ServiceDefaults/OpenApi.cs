namespace BookWorm.ServiceDefaults;

public sealed class OpenApi
{
    public Document Document { get; set; } = new();
}

public sealed class Document
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
