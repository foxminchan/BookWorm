using System.Security.Claims;

namespace BookWorm.Chassis.Security.TokenExchange;

public interface ITokenExchange
{
    /// <summary>
    ///     Exchanges a subject token for a new token using Keycloak's token-exchange grant.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal containing the subject token.</param>
    /// <param name="audience">Optional requested audience for the exchanged token.</param>
    /// <param name="scope">Optional scope to request on the exchanged token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchanged access token.</returns>
    Task<string> ExchangeAsync(
        ClaimsPrincipal claimsPrincipal,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Exchanges a supplied subject token for a token with the requested audience and scope.
    /// </summary>
    /// <param name="subjectToken">The access token to exchange.</param>
    /// <param name="audience">Optional requested audience for the exchanged token.</param>
    /// <param name="scope">Optional scope to request on the exchanged token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchanged access token.</returns>
    Task<string> ExchangeAsync(
        string subjectToken,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Gets a client-credentials token and exchanges it for the requested audience and scope.
    /// </summary>
    /// <param name="audience">Optional requested audience for the exchanged token.</param>
    /// <param name="scope">Optional scope to request on the exchanged token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchanged access token.</returns>
    Task<string> ExchangeClientCredentialsAsync(
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    );
}
