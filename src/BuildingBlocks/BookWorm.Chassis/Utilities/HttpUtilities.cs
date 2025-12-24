using System.Text;
using BookWorm.Constants.Core;
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
    private string? _host;
    private int? _port;
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
        _scheme = scheme.ToLowerInvariant();
        return this;
    }

    public UrlBuilder WithHost(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        _host = host.Trim();
        return this;
    }

    public UrlBuilder WithHost(HostString host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host.Value);
        _host = host.Value;
        return this;
    }

    public UrlBuilder WithPort(int port)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);
        _port = port;
        return this;
    }

    public UrlBuilder WithPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return this;
        }

        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            if (segment.Length > 0)
            {
                _pathSegments.Add(segment);
            }
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
        _fragment = fragment.TrimStart('#');
        return this;
    }

    public string Build()
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            builder.Append(_baseUrl);
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(_scheme);
            ArgumentException.ThrowIfNullOrWhiteSpace(_host);

            builder.Append(_scheme);
            builder.Append("://");
            builder.Append(_host);

            if (_port.HasValue && !IsDefaultPort(_scheme, _port.Value))
            {
                builder.Append(':');
                builder.Append(_port.Value);
            }
        }

        if (_pathSegments.Count > 0)
        {
            builder.Append('/');
            builder.Append(string.Join('/', _pathSegments));
        }

        if (_query.Count > 0)
        {
            builder.Append('?');
            builder.Append(
                string.Join(
                    '&',
                    _query.Select(x =>
                        string.IsNullOrWhiteSpace(x.Value)
                            ? Uri.EscapeDataString(x.Key)
                            : $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"
                    )
                )
            );
        }

        if (string.IsNullOrWhiteSpace(_fragment))
        {
            return builder.ToString();
        }

        builder.Append('#');
        builder.Append(Uri.EscapeDataString(_fragment));

        return builder.ToString();
    }

    private static bool IsDefaultPort(string scheme, int port)
    {
        return (scheme == Http.Schemes.Https && port == 443)
            || (scheme == Http.Schemes.Http && port == 80);
    }

    public override string ToString()
    {
        return Build();
    }

    public static implicit operator string(UrlBuilder urlBuilder)
    {
        return urlBuilder.Build();
    }
}
