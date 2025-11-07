using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Constants.Aspire;

namespace BookWorm.Chassis.Security.TokenExchange;

public sealed class TokenExchange(
    IHttpClientFactory httpClientFactory,
    IdentityOptions identityOptions
) : ITokenExchange
{
    public async Task<TokenExchangeResult> ExchangeAsync(
        string subjectToken,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    )
    {
        var tokenEndpoint = KeycloakEndpoints
            .Token.Replace("{realm}", identityOptions.Realm)
            .TrimStart('/');

        using var httpClient = httpClientFactory.CreateClient(Components.KeyCloak);

        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"),
            new("subject_token", subjectToken),
            new("subject_token_type", "urn:ietf:params:oauth:token-type:access_token"),
        };

        if (!string.IsNullOrWhiteSpace(audience))
        {
            parameters.Add(new("audience", audience));
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            parameters.Add(new("scope", scope));
        }

        using var content = new FormUrlEncodedContent(parameters);

        var response = await httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Token exchange failed: {response.StatusCode} {response.ReasonPhrase}. Response: {responseBody}"
            );
        }

        var tokenResult = JsonSerializer.Deserialize(
            responseBody,
            TokenExchangeJsonContext.Default.TokenExchangeResult
        );

        if (string.IsNullOrWhiteSpace(tokenResult?.AccessToken))
        {
            throw new InvalidOperationException(
                $"Token exchange did not return an access_token. Response: {responseBody}"
            );
        }

        return tokenResult;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TokenExchangeResult))]
internal partial class TokenExchangeJsonContext : JsonSerializerContext;
