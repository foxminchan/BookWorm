using System.Text;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Chassis.Utilities;

public static class HttpUtilities
{
    /// <summary>
    ///     Creates a new URL builder for constructing URLs with a fluent API.
    /// </summary>
    /// <returns>A new instance of <see cref="UrlBuilder" />.</returns>
    /// <example>
    ///     <code>
    /// var url = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("api.example.com")
    ///     .WithPath("v1")
    ///     .WithPath("users")
    ///     .WithQuery("page", "1")
    ///     .Build();
    /// </code>
    /// </example>
    public static UrlBuilder BuildUrl()
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

    /// <summary>
    ///     Sets the base URL. Cannot be combined with scheme/host/port.
    /// </summary>
    /// <param name="baseUrl">The base URL including scheme and host (e.g., "https://example.com", "http://localhost:8080").</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the base URL is invalid or not an absolute URI.</exception>
    /// <remarks>
    ///     <para>
    ///         When using <c>WithBase</c>, you cannot separately specify scheme, host, or port as they are
    ///         already part of the base URL. You can still add path segments, query parameters, and fragments.
    ///     </para>
    ///     <para>
    ///         Trailing slashes are automatically removed from the base URL to ensure proper path joining.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// var url = HttpUtilities.BuildUrl()
    ///     .WithBase("https://api.example.com")
    ///     .WithPath("v1/users")
    ///     .WithQuery("active", "true")
    ///     .Build();
    /// // Result: https://api.example.com/v1/users?active=true
    /// </code>
    /// </example>
    public UrlBuilder WithBase(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid base URL.", nameof(baseUrl));
        }

        _baseUrl = baseUrl.TrimEnd('/');
        return this;
    }

    /// <summary>
    ///     Sets the URL scheme (protocol).
    /// </summary>
    /// <param name="scheme">The scheme without the "://" separator (e.g., "http", "https", "ftp", "ws").</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when scheme is null or whitespace.</exception>
    /// <remarks>
    ///     The scheme is automatically converted to lowercase as per RFC 3986.
    ///     Common schemes include "http", "https", "ftp", "ws", "wss".
    /// </remarks>
    /// <example>
    ///     <code>
    /// var url = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .Build();
    /// // Result: https://example.com
    /// </code>
    /// </example>
    public UrlBuilder WithScheme(string scheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);
        _scheme = scheme.ToLowerInvariant();
        return this;
    }

    /// <summary>
    ///     Sets the host name.
    /// </summary>
    /// <param name="host">The host name or IP address (e.g., "example.com", "api.example.com", "192.168.1.1", "[::1]").</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when host is null or whitespace.</exception>
    /// <remarks>
    ///     <para>
    ///         The host can be a domain name, IPv4 address, or IPv6 address (enclosed in brackets).
    ///         Leading and trailing whitespace is automatically trimmed.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Using domain name
    /// var url1 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("api.example.com")
    ///     .Build();
    ///
    /// // Using IPv4
    /// var url2 = HttpUtilities.BuildUrl()
    ///     .WithScheme("http")
    ///     .WithHost("192.168.1.1")
    ///     .WithPort(8080)
    ///     .Build();
    /// </code>
    /// </example>
    public UrlBuilder WithHost(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        _host = host.Trim();
        return this;
    }

    /// <summary>
    ///     Sets the host from an ASP.NET Core <see cref="HostString" />.
    /// </summary>
    /// <param name="host">The <see cref="HostString" /> containing the host name and optional port.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when host value is null or whitespace.</exception>
    /// <remarks>
    ///     <para>
    ///         This overload is useful when working with ASP.NET Core's <c>HttpRequest.Host</c> property.
    ///         If the <see cref="HostString" /> includes a port, it will be included in the host value.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // In an ASP.NET Core controller or middleware
    /// var url = HttpUtilities.BuildUrl()
    ///     .WithScheme(HttpContext.Request.Scheme)
    ///     .WithHost(HttpContext.Request.Host)
    ///     .WithPath("api/users")
    ///     .Build();
    /// </code>
    /// </example>
    /// <seealso cref="HostString" />
    public UrlBuilder WithHost(HostString host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host.Value);
        _host = host.Value;
        return this;
    }

    /// <summary>
    ///     Sets the port number. Default ports (80 for HTTP, 443 for HTTPS) are omitted in the final URL.
    /// </summary>
    /// <param name="port">The port number (must be between 1 and 65535).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when port is negative or zero.</exception>
    /// <remarks>
    ///     <para>
    ///         Standard ports are automatically omitted from the final URL:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Port 80 for HTTP scheme</description>
    ///         </item>
    ///         <item>
    ///             <description>Port 443 for HTTPS scheme</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         For all other scheme/port combinations, the port is explicitly included in the URL.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Port 443 is omitted for HTTPS
    /// var url1 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .WithPort(443)
    ///     .Build();
    /// // Result: https://example.com
    ///
    /// // Non-standard port is included
    /// var url2 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .WithPort(8443)
    ///     .Build();
    /// // Result: https://example.com:8443
    /// </code>
    /// </example>
    public UrlBuilder WithPort(int port)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);
        _port = port;
        return this;
    }

    /// <summary>
    ///     Adds a path segment to the URL. Can be called multiple times to build up the path.
    ///     Leading and trailing slashes are automatically handled.
    /// </summary>
    /// <param name="path">
    ///     The path segment to add. Can include slashes which will be trimmed (e.g., "api", "/users", "v1/",
    ///     "users/123").
    /// </param>
    /// <returns>This builder instance for chaining.</returns>
    /// <remarks>
    ///     <para>
    ///         This method can be called multiple times to build up a path. Each segment is properly encoded
    ///         according to RFC 3986 when the final URL is built.
    ///     </para>
    ///     <para>
    ///         Leading and trailing slashes are automatically trimmed from each segment to prevent double slashes.
    ///         Empty or whitespace-only segments are ignored.
    ///     </para>
    ///     <para>
    ///         Special characters in path segments (spaces, non-ASCII characters, etc.) are percent-encoded.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Building path with multiple calls
    /// var url1 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("api.example.com")
    ///     .WithPath("v1")
    ///     .WithPath("users")
    ///     .WithPath("123")
    ///     .Build();
    /// // Result: https://api.example.com/v1/users/123
    ///
    /// // Slashes are handled automatically
    /// var url2 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .WithPath("/api/")
    ///     .WithPath("/v1/")
    ///     .Build();
    /// // Result: https://example.com/api/v1
    ///
    /// // Special characters are encoded
    /// var url3 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .WithPath("hello world")
    ///     .Build();
    /// // Result: https://example.com/hello%20world
    /// </code>
    /// </example>
    public UrlBuilder WithPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return this;
        }

        var segment = path.Trim('/');

        if (segment.Length > 0)
        {
            _pathSegments.Add(segment);
        }

        return this;
    }

    /// <summary>
    ///     Adds a query parameter to the URL. Can be called multiple times to add multiple parameters.
    /// </summary>
    /// <param name="key">The query parameter key. Keys are case-insensitive.</param>
    /// <param name="value">The query parameter value. If <c>null</c> or empty, only the key is added (flag parameter).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    /// <remarks>
    ///     <para>
    ///         Query parameter keys are case-insensitive. If you add a parameter with the same key multiple times,
    ///         the last value will be used.
    ///     </para>
    ///     <para>
    ///         Both keys and values are percent-encoded according to RFC 3986 when building the final URL.
    ///     </para>
    ///     <para>
    ///         If the value is <c>null</c> or empty, the parameter appears as a flag (key only, no equals sign or value).
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Adding multiple query parameters
    /// var url1 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("api.example.com")
    ///     .WithPath("search")
    ///     .WithQuery("q", "hello world")
    ///     .WithQuery("page", "1")
    ///     .WithQuery("size", "10")
    ///     .Build();
    /// // Result: https://api.example.com/search?q=hello%20world&amp;page=1&amp;size=10
    ///
    /// // Using flag parameter (no value)
    /// var url2 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .WithPath("items")
    ///     .WithQuery("includeInactive", null)
    ///     .Build();
    /// // Result: https://example.com/items?includeInactive
    /// </code>
    /// </example>
    public UrlBuilder WithQuery(string key, string? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _query[key] = value;
        return this;
    }

    /// <summary>
    ///     Sets the URL fragment (hash).
    /// </summary>
    /// <param name="fragment">The fragment identifier without the leading '#' symbol.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when fragment is null or whitespace.</exception>
    /// <remarks>
    ///     <para>
    ///         The fragment appears at the end of the URL after the '#' symbol and is typically used
    ///         for client-side navigation or to identify a specific section of a document.
    ///     </para>
    ///     <para>
    ///         The leading '#' is automatically added if not present, and the fragment is percent-encoded
    ///         according to RFC 3986.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// var url = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .WithPath("docs")
    ///     .WithFragment("section-1")
    ///     .Build();
    /// // Result: https://example.com/docs#section-1
    /// </code>
    /// </example>
    public UrlBuilder WithFragment(string fragment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fragment);
        _fragment = fragment.TrimStart('#');
        return this;
    }

    /// <summary>
    ///     Builds the final URL string with proper encoding.
    /// </summary>
    /// <returns>The constructed, properly encoded URL string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scheme or host is not provided and no base URL is set.</exception>
    /// <remarks>
    ///     <para>
    ///         This method validates that all required components are present and constructs the final URL
    ///         with proper percent-encoding of all components according to RFC 3986:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Path segments are percent-encoded</description>
    ///         </item>
    ///         <item>
    ///             <description>Query parameter keys and values are percent-encoded</description>
    ///         </item>
    ///         <item>
    ///             <description>Fragment is percent-encoded</description>
    ///         </item>
    ///         <item>
    ///             <description>Default ports (80 for HTTP, 443 for HTTPS) are omitted</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         You can also use implicit conversion to <see cref="string" /> instead of calling this method explicitly.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Explicit Build() call
    /// var url1 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com")
    ///     .Build();
    ///
    /// // Implicit conversion
    /// string url2 = HttpUtilities.BuildUrl()
    ///     .WithScheme("https")
    ///     .WithHost("example.com");
    /// </code>
    /// </example>
    public string Build()
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            builder.Append(_baseUrl);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(
                _scheme,
                "Scheme is required when not using base URL"
            );
            ArgumentNullException.ThrowIfNull(_host, "Host is required when not using base URL");

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
            builder.Append(string.Join('/', _pathSegments.Select(Uri.EscapeDataString)));
        }

        if (_query.Count > 0)
        {
            builder.Append('?');
            builder.Append(
                string.Join(
                    '&',
                    _query.Select(x =>
                        string.IsNullOrEmpty(x.Value)
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
