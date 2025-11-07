using System.Text.Json;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Constants.Aspire;

namespace BookWorm.Chassis.Security.TokenExchange;

public sealed class TokenExchange(
    IHttpClientFactory httpClientFactory,
    IdentityOptions identityOptions
) : ITokenExchange
{
    public async Task<string> ExchangeAsync(
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

        using var requestContent = new FormUrlEncodedContent(parameters);

        var response = await httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Token exchange failed: {response.StatusCode} {response.ReasonPhrase}"
            );
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var tokenResponse = JsonDocument.Parse(content);

        if (
            !tokenResponse.RootElement.TryGetProperty("access_token", out var accessTokenElement)
            || string.IsNullOrWhiteSpace(accessTokenElement.GetString())
        )
        {
            throw new UnauthorizedAccessException("Token exchange did not return an access_token");
        }

        var accessToken = accessTokenElement.GetString()!;

        return accessToken;
    }
}
