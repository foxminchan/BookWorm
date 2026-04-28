using System.Security.Claims;
using System.Text.Json;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Constants.Aspire;

namespace BookWorm.Chassis.Security.TokenExchange;

internal sealed class TokenExchange(
    IHttpClientFactory httpClientFactory,
    IdentityOptions identityOptions
) : ITokenExchange
{
    public async Task<string> ExchangeAsync(
        ClaimsPrincipal claimsPrincipal,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    )
    {
        var tokenEndpoint = KeycloakEndpoints.Token(identityOptions.Realm).TrimStart('/');

        var requestContent = GetRequestContent(claimsPrincipal, audience, scope);

        using var httpClient = httpClientFactory.CreateClient(Components.KeyCloak);

        var response = await httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Token exchange failed: {response.StatusCode} {response.ReasonPhrase}"
            );
        }

        return await GetResponseContent(response, cancellationToken);
    }

    private FormUrlEncodedContent GetRequestContent(
        ClaimsPrincipal claimsPrincipal,
        string? audience = null,
        string? scope = null
    )
    {
        var tokenClaim = claimsPrincipal.FindFirst("access_token");

        if (string.IsNullOrWhiteSpace(tokenClaim?.Value))
        {
            throw new UnauthorizedAccessException("No access_token found in claims principal");
        }

        var parameters = new List<KeyValuePair<string, string>>
        {
            new("client_id", identityOptions.ClientId),
            new("client_secret", identityOptions.ClientSecret),
            new("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"),
            new("subject_token", tokenClaim.Value),
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

        return new(parameters);
    }

    private static async Task<string> GetResponseContent(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var tokenResponse = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken
        );

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
