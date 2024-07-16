namespace BookWorm.Identity.Options;

public sealed class ServiceOptions
{
    public string Bff { get; set; } = string.Empty;

    public string Catalog { get; set; } = string.Empty;

    public string Ordering { get; set; } = string.Empty;

    public string Basket { get; set; } = string.Empty;

    public string Rating { get; set; } = string.Empty;

    public string StoreFront { get; set; } = string.Empty;

    public string BackOffice { get; set; } = string.Empty;
}
