using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiOptionsExtensions
{
    extension(OpenApiOptions options)
    {
        public void ApplyApiInfo()
        {
            options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer>();
        }

        public void ApplySecuritySchemeDefinitions()
        {
            options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();
        }

        public void ApplyAuthorizationChecks()
        {
            options.AddOperationTransformer<AuthorizationChecksTransformer>();
        }
    }
}
