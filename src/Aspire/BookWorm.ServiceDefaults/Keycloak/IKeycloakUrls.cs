namespace BookWorm.ServiceDefaults.Keycloak;

public interface IKeycloakUrls
{
    Task<string> GetAccountConsoleUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );

    Task<string> GetTokenUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );

    Task<string> GetAuthorizationUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );
}
