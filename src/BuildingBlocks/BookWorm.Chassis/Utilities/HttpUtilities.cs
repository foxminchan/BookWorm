using Microsoft.AspNetCore.Http;

namespace BookWorm.Chassis.Utilities;

public static class HttpUtilities
{
    public static UrlBuilder AsUrlBuilder()
    {
        return new();
    }
}

public sealed class UrlBuilder
{
    private readonly List<string> _pathSegments = [];
    private readonly Dictionary<string, string?> _query = new(StringComparer.OrdinalIgnoreCase);
    private string? _baseUrl;
    private string? _fragment;
    private HostString _host;
    private string? _scheme;

    internal UrlBuilder() { }

    public UrlBuilder WithBase(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid base URL.", nameof(baseUrl));
        }

        _baseUrl = baseUrl.TrimEnd('/');
        return this;
    }

    public UrlBuilder WithScheme(string scheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);
        _scheme = scheme;
        return this;
    }

    public UrlBuilder WithHost(string host)
    {
        return WithHost(new HostString(host.Trim()));
    }

    public UrlBuilder WithHost(HostString host)
    {
        if (!host.HasValue)
        {
            throw new ArgumentException("Host cannot be empty.", nameof(host));
        }

        _host = host;
        return this;
    }

    public UrlBuilder WithPort(int port)
    {
        _host = new(_host.Host, port);
        return this;
    }

    public UrlBuilder WithPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return this;
        }

        foreach (var segment in path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            _pathSegments.Add(segment);
        }

        return this;
    }

    public UrlBuilder WithQuery(string key, string? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _query[key] = value;
        return this;
    }

    public UrlBuilder WithFragment(string fragment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fragment);
        _fragment = fragment.StartsWith('#') ? fragment[1..] : fragment;
        return this;
    }

    public UrlBuilder WithFragment(FragmentString fragment)
    {
        _fragment = fragment.HasValue ? fragment.Value.TrimStart('#') : null;
        return this;
    }

    public string Build()
    {
        var inner = CreateUriBuilder();

        if (_pathSegments.Count > 0)
        {
            var basePath = inner.Path.TrimEnd('/');
            inner.Path = $"{basePath}/{string.Join('/', _pathSegments)}";
        }

        if (_query.Count > 0)
        {
            inner.Query = string.Join(
                '&',
                _query.Select(x =>
                    string.IsNullOrWhiteSpace(x.Value)
                        ? Uri.EscapeDataString(x.Key)
                        : $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"
                )
            );
        }

        if (_fragment is not null)
        {
            inner.Fragment = _fragment;
            inner.Fragment = _fragment;
        }

        var url = inner.ToString();

        // UriBuilder always appends "/" for root path; strip for scheme+host-only URLs
        if (inner.Path is "/" && _pathSegments.Count == 0)
        {
            url = url.TrimEnd('/');
        }

        return url;
    }

    public override string ToString()
    {
        return Build();
    }

    public static implicit operator string(UrlBuilder urlBuilder)
    {
        return urlBuilder.Build();
    }

    private UriBuilder CreateUriBuilder()
    {
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            var builder = new UriBuilder(_baseUrl);

            if (IsDefaultPort(builder.Scheme, builder.Port))
            {
                builder.Port = -1;
            }

            return builder;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(_scheme);

        if (!_host.HasValue)
        {
            throw new ArgumentException("Host is required.");
        }

        var inner = new UriBuilder { Scheme = _scheme, Host = _host.Host };

        if (_host.Port is { } port && !IsDefaultPort(_scheme, port))
        {
            inner.Port = port;
        }

        return inner;
    }

    private static bool IsDefaultPort(string scheme, int port)
    {
        return (scheme == Uri.UriSchemeHttps && port == 443)
            || (scheme == Uri.UriSchemeHttp && port == 80);
    }
}
