using Duende.Bff;

namespace BookWorm.Web.Bff;

/// <summary>
///     Configuration section
/// </summary>
public class Configuration
{
    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    /// <summary>
    ///     should be supplied as a command line argument or environment variable, e.g.
    ///     ./GenericBFF --BFF:ClientSecret=secret
    /// </summary>
    public string? ClientSecret { get; set; }

    public List<string> Scopes { get; set; } = [];
    public List<Api> Apis { get; set; } = [];
}

public class Api
{
    public string? LocalPath { get; set; }
    public string? RemoteUrl { get; set; }
    public TokenType RequiredToken { get; set; }
}
