namespace BookWorm.AppHost.Extensions.Proxy;

internal sealed class FrontendRoute
{
    public required IResourceBuilder<IResourceWithEndpoints> Resource { get; init; }
    public string? PathPrefix { get; init; }
    public string? Pattern { get; init; }
    public bool IsExact { get; init; }
    public int? Order { get; init; }
    public string? ZoneName { get; init; }
}
