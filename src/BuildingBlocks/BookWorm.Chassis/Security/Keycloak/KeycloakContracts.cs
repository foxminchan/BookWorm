using System.Text.Json.Serialization;

namespace BookWorm.Chassis.Security.Keycloak;

internal sealed record KeycloakTokenResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken
);

internal sealed record KeycloakIntrospectionResponse(
    [property: JsonPropertyName("active")] bool Active
);

[JsonSerializable(typeof(KeycloakTokenResponse))]
[JsonSerializable(typeof(KeycloakIntrospectionResponse))]
internal sealed partial class KeycloakSerializationContext : JsonSerializerContext;
