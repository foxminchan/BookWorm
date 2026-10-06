using System.Security.Claims;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;

namespace BookWorm.Chassis.Security.TokenExchange;

internal sealed class TokenExchange(IKeycloakApi keycloakApi, IdentityOptions identityOptions)
    : ITokenExchange
{
    public async Task<string> ExchangeAsync(
        ClaimsPrincipal claimsPrincipal,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    )
    {
        var requestContent = GetRequestContent(claimsPrincipal, audience, scope);
        using var response = await keycloakApi.ExchangeTokenAsync(
            identityOptions.Realm,
            requestContent,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Token exchange failed: {response.StatusCode} {response.ReasonPhrase}"
            );
        }

        await response.EnsureSuccessfulAsync();

        var accessToken = response.Content?.AccessToken;

        return string.IsNullOrWhiteSpace(accessToken)
            ? throw new UnauthorizedAccessException("Token exchange did not return an access_token")
            : accessToken;
    }

    private Dictionary<string, string> GetRequestContent(
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

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = identityOptions.ClientId,
            ["client_secret"] = identityOptions.ClientSecret,
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
            ["subject_token"] = tokenClaim.Value,
            ["subject_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
        };

        if (!string.IsNullOrWhiteSpace(audience))
        {
            parameters.Add("audience", audience);
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            parameters.Add("scope", scope);
        }

        return parameters;
    }
}
