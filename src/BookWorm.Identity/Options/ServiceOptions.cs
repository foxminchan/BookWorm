namespace BookWorm.Identity.Options;

public sealed class ServiceOptions
{
    public string Bff { get; set; } = string.Empty;

    public string StoreFront { get; set; } = string.Empty;

    public string BackOffice { get; set; } = string.Empty;
}
