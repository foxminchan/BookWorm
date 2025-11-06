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

        var authUrlBuilder = new StringBuilder();
        authUrlBuilder.Append(keycloakUrl);
        authUrlBuilder.Append('/');
        authUrlBuilder.Append(
            KeycloakEndpoints.Authorize.Replace("{realm}", identityOptions.Realm).TrimStart('/')
        );

        // Please refer: https://github.com/scalar/scalar/issues/6225
        var tokenUrlBuilder = new StringBuilder();
        tokenUrlBuilder.Append(Protocols.Http);
        tokenUrlBuilder.Append("://");
        tokenUrlBuilder.Append(Components.KeyCloak);
        tokenUrlBuilder.Append('/');
        tokenUrlBuilder.Append(
            KeycloakEndpoints.Token.Replace("{realm}", identityOptions.Realm).TrimStart('/')
        );

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 security scheme for the BookWorm API",
            Flows = new()
            {
                AuthorizationCode = new()
                {
                    Scopes = identityOptions.Scopes!,
                    AuthorizationUrl = new(authUrlBuilder.ToString()),
                    TokenUrl = new(tokenUrlBuilder.ToString()),
                },
            },
        };

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add(OAuthDefaults.DisplayName, securityScheme);

        return Task.CompletedTask;
    }
}
