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
        var tokenClaim = claimsPrincipal.FindFirst("access_token");

        if (string.IsNullOrWhiteSpace(tokenClaim?.Value))
        {
            throw new UnauthorizedAccessException("No access_token found in claims principal");
        }

        return await ExchangeAsync(tokenClaim.Value, audience, scope, cancellationToken);
    }

    public async Task<string> ExchangeAsync(
        string subjectToken,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(subjectToken))
        {
            throw new UnauthorizedAccessException("The subject token is required");
        }

        var requestContent = GetRequestContent(subjectToken, audience, scope);
        using var response = await keycloakApi.ExchangeTokenAsync(
            identityOptions.Realm,
            requestContent,
            cancellationToken
        );

        return await ReadAccessTokenAsync(response, "Token exchange");
    }

    public async Task<string> ExchangeClientCredentialsAsync(
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    )
    {
        var clientCredentials = new Dictionary<string, string>
        {
            ["client_id"] = identityOptions.ClientId,
            ["client_secret"] = identityOptions.ClientSecret,
            ["grant_type"] = "client_credentials",
        };

        string subjectToken;
        using (
            var response = await keycloakApi.ExchangeTokenAsync(
                identityOptions.Realm,
                clientCredentials,
                cancellationToken
            )
        )
        {
            subjectToken = await ReadAccessTokenAsync(response, "Client-credentials grant");
        }

        return await ExchangeAsync(subjectToken, audience, scope, cancellationToken);
    }

    private Dictionary<string, string> GetRequestContent(
        string subjectToken,
        string? audience,
        string? scope
    )
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = identityOptions.ClientId,
            ["client_secret"] = identityOptions.ClientSecret,
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
            ["subject_token"] = subjectToken,
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

    private static async Task<string> ReadAccessTokenAsync(
        Refit.IApiResponse<KeycloakTokenResponse> response,
        string operation
    )
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"{operation} failed: {response.StatusCode} {response.ReasonPhrase}"
            );
        }

        await response.EnsureSuccessfulAsync();

        var accessToken = response.Content?.AccessToken;

        return string.IsNullOrWhiteSpace(accessToken)
            ? throw new UnauthorizedAccessException($"{operation} did not return an access_token")
            : accessToken;
    }
}
