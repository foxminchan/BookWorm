namespace BookWorm.Chassis.Security.TokenAcquisition;

public interface ITokenExchange
{
    /// <summary>
    /// Exchanges a subject token for a new token using Keycloak's token-exchange grant.
    /// </summary>
    /// <param name="subjectToken">The token to exchange (usually an access token).</param>
    /// <param name="audience">Optional requested audience for the exchanged token.</param>
    /// <param name="scope">Optional scope to request on the exchanged token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="TokenExchangeResult"/> describing the exchanged token or an error.</returns>
    Task<TokenExchangeResult> ExchangeAsync(
        string subjectToken,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    );
}
