using Aspire.Hosting.Dotnet;

namespace BookWorm.AppHost.Extensions.Proxy;

internal sealed class Service
{
    public const long DefaultMaxRequestBodySize = 10 * 1024 * 1024;
    public required string Name { get; init; }
    public required IResourceBuilder<DotnetProjectResource> Resource { get; init; }
    public long MaxRequestBodySize { get; init; } = DefaultMaxRequestBodySize;
    public int? Order { get; init; }
}
