using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiOptionsExtensions
{
    extension(OpenApiOptions options)
    {
        public void ApplyApiInfo(DocumentOptions? openApiDocument)
        {
            options.AddDocumentTransformer(new OpenApiInfoDefinitionsTransformer(openApiDocument));
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
