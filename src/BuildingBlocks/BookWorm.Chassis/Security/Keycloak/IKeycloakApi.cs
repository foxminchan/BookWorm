using Refit;

namespace BookWorm.Chassis.Security.Keycloak;

internal interface IKeycloakApi
{
    [Post("/realms/{realm}/protocol/openid-connect/token")]
    Task<ApiResponse<KeycloakTokenResponse>> ExchangeTokenAsync(
        string realm,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default
    );

    [Post("/realms/{realm}/protocol/openid-connect/token/introspect")]
    Task<ApiResponse<KeycloakIntrospectionResponse>> IntrospectTokenAsync(
        string realm,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default
    );
}
