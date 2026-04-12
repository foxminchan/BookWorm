using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Maps the default OpenAPI endpoints for local development.
        /// </summary>
        /// <remarks>
        /// This is intentionally enabled only in development to avoid exposing
        /// API specification endpoints in non-development environments.
        /// </remarks>
        public void UseDefaultOpenApi()
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapGet("/", () => TypedResults.Redirect("openapi/v1.json"))
                    .ExcludeFromDescription();
            }
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the default OpenAPI configuration for the service.
        /// </summary>
        /// <param name="configure">
        /// Optional callback to customize <see cref="OpenApiOptions" /> after the default transformers are applied.
        /// </param>
        /// <remarks>
        /// Adds authorization metadata and security scheme definitions to keep API documentation consistent across services.
        /// </remarks>
        public void AddDefaultOpenApi(Action<OpenApiOptions>? configure = null)
        {
            services.AddOpenApi(options =>
            {
                options.AddOperationTransformer<AuthorizationChecksTransformer>();
                options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();
                configure?.Invoke(options);
            });
        }
    }
}
