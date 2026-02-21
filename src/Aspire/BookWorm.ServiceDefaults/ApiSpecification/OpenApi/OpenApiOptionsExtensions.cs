using Asp.Versioning.ApiExplorer;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiOptionsExtensions
{
    extension(OpenApiOptions options)
    {
        public void ApplyApiVersionInfo(
            DocumentOptions? openApiDocument,
            ApiVersionDescription apiDescription
        )
        {
            options.AddDocumentTransformer(
                new OpenApiInfoDefinitionsTransformer(openApiDocument, apiDescription)
            );
        }

        public void ApplySecuritySchemeDefinitions()
        {
            options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();
        }

        public void ApplyAuthorizationChecks(string[] scopes)
        {
            options.AddOperationTransformer(new AuthorizationChecksTransformer(scopes));
        }
    }
}
