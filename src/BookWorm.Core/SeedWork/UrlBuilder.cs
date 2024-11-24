namespace BookWorm.Core.SeedWork;

public sealed class UrlBuilder
{
    private int? _version;
    private string? _resource;
    private string? _id;

    public UrlBuilder WithVersion(int? version = 1)
    {
        _version = version;
        return this;
    }

    public UrlBuilder WithResource(string resource)
    {
        _resource = resource.ToLowerInvariant();
        return this;
    }

    public UrlBuilder WithId<T>(T id)
    {
        _id = id?.ToString();
        return this;
    }

    public string Build()
    {
        return _version is not null
            ? $"/api/{_version}/{_resource}/{_id}"
            : $"/api/{_resource}/{_id}";
    }
}
