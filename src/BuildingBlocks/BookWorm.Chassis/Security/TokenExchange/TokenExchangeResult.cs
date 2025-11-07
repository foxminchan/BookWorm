using System.Text.Json.Serialization;

namespace BookWorm.Chassis.Security.TokenExchange;

public sealed class TokenExchangeResult
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
