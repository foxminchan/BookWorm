using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
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

    public static void UseDefaultOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapGet("/", () => TypedResults.Redirect("openapi/v1.json"))
                .ExcludeFromDescription();
        }
    }
}
