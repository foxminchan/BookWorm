using BookWorm.Chassis.Security.Settings;
using Microsoft.AspNetCore.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    extension(IServiceCollection services)
    {
        public void AddDefaultOpenApi()
        {
            var sp = services.BuildServiceProvider();
            var identity = sp.GetService<IdentityOptions>();

            services.AddSimpleOpenApi(options =>
            {
                if (identity is not null)
                {
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyAuthorizationChecks([.. identity.Scopes.Keys]);
                }
            });
        }

        public void AddSimpleOpenApi(Action<OpenApiOptions>? configure = null)
        {
            var sp = services.BuildServiceProvider();
            var document = sp.GetRequiredService<DocumentOptions>();

            services.AddOpenApi(options =>
            {
                options.ApplyApiInfo(document);
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
