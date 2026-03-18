using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    public static void UseDefaultOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // AllowAnonymous ensures OpenAPI spec and redirect are accessible even when a
            // FallbackPolicy requires authentication — developer tooling should not be blocked
            app.MapOpenApi().AllowAnonymous();
            app.MapGet("/", () => TypedResults.Redirect("openapi/v1.json"))
                .ExcludeFromDescription()
                .AllowAnonymous();
        }
    }

    extension(IServiceCollection services)
    {
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
