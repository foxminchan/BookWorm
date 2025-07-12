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

    Task<string> GetIntrospectionUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );

    Task<string> GetUserInfoUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );

    Task<string> GetEndSessionUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );

    Task<string> GetRegistrationUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );

    Task<string> GetMetadataUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    );
}
