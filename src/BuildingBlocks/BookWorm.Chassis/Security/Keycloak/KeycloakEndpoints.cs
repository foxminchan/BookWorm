namespace BookWorm.Chassis.Security.Keycloak;

public static class KeycloakEndpoints
{
    public const string Token = "/realms/{realm}/protocol/openid-connect/token";
    public const string Authorize = "/realms/{realm}/protocol/openid-connect/auth";
    public const string Introspect = "/realms/{realm}/protocol/openid-connect/token/introspect";
}
