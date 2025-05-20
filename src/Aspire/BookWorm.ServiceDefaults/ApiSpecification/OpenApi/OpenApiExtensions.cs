using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    public static void AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var document = builder.Configuration.GetSection(nameof(Document)).Get<Document>();

        Span<string> versions = ["v1"];

        foreach (var description in versions)
        {
            services.AddOpenApi(
                description,
                options =>
                {
                    options.ApplyApiVersionInfo(document);
                    options.ApplySchemaNullableFalse();
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyOperationDeprecatedStatus();
                }
            );
        }
    }

    public static void UseDefaultOpenApi(this WebApplication app)
    {
        app.MapOpenApi();

        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapScalarApiReference(options =>
        {
            options.Theme = ScalarTheme.BluePlanet;
            options.DefaultFonts = false;
        });

        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
    }
}
