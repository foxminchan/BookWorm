using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;
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

        var authUrl = HttpUtilities
            .AsUrlBuilder()
            .WithBase(keycloakUrl)
            .WithPath(KeycloakEndpoints.Authorize.Replace("{realm}", identityOptions.Realm))
            .Build();

        var tokenUrl = HttpUtilities
            .AsUrlBuilder()
            .WithScheme(Http.Schemes.Http)
            .WithHost(Components.KeyCloak)
            .WithPath(KeycloakEndpoints.Token.Replace("{realm}", identityOptions.Realm))
            .Build();

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 security scheme for the BookWorm API",
            Flows = new()
            {
                AuthorizationCode = new()
                {
                    Scopes = identityOptions.Scopes!,
                    AuthorizationUrl = new(authUrl),
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
