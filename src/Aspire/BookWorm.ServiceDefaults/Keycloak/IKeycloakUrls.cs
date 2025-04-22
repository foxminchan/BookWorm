namespace BookWorm.ServiceDefaults.Keycloak;

public interface IKeycloakUrls
{
    Task<string> GetAccountConsoleUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );
}
