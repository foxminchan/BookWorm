using BookWorm.Chassis.Utilities;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class SecuritySchemeDefinitionsTransformer(IdentityOptions identityOptions)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var keycloakUrl = ServiceDiscoveryUtilities.GetServiceEndpoint(Components.KeyCloak);

        if (string.IsNullOrWhiteSpace(keycloakUrl))
        {
            return Task.CompletedTask;
        }

        var realmPath = $"realms/{identityOptions.Realm}";

        var authorizationUrl = $"{keycloakUrl}/{realmPath}/protocol/openid-connect/auth";

        // Please refer: https://github.com/scalar/scalar/issues/6225
        var tokenUrl =
            $"{Protocols.Http}://{Components.KeyCloak}/{realmPath}/protocol/openid-connect/token";

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 security scheme for the BookWorm API",
            Flows = new()
            {
                AuthorizationCode = new()
                {
                    Scopes = identityOptions.Scopes!,
                    AuthorizationUrl = new(authorizationUrl),
                    TokenUrl = new(tokenUrl),
                },
            },
        };

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add(OAuthDefaults.DisplayName, securityScheme);

        return Task.CompletedTask;
    }
}
