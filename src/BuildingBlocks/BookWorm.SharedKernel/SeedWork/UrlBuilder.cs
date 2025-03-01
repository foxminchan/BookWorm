namespace BookWorm.SharedKernel.SeedWork;

public sealed class UrlBuilder
{
    private string? _id;
    private string? _resource;
    private int? _version;

    public UrlBuilder WithVersion(int? version = 1)
    {
        _version = version;
        return this;
    }

    public UrlBuilder WithResource(string resource)
    {
        _resource = resource.ToLowerInvariant();

        if (resource.EndsWith('s'))
        {
            return this;
        }

        _resource += "s";

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
