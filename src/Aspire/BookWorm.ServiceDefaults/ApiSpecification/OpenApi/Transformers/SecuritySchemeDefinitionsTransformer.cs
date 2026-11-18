using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class SecuritySchemeDefinitionsTransformer(IConfiguration configuration)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var scopes = configuration[$"{Microsoft.Identity.Web.Constants.AzureAd}:Scopes"]
            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToDictionary(scope => scope, scope => scope);

        if (scopes is not { Count: > 0 })
        {
            return Task.CompletedTask;
        }

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 security scheme for the BookWorm API",
            Flows = new() { AuthorizationCode = new() { Scopes = scopes } },
        };

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add(OAuthDefaults.DisplayName, securityScheme);

        return Task.CompletedTask;
    }
}
