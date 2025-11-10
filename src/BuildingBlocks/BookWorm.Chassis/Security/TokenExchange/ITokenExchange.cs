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
}
