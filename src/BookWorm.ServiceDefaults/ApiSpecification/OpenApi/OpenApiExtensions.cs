using APIWeaver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    public static void AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var document = builder.Configuration.GetSection(nameof(Document)).Get<Document>();

        string[] versions = ["v1"];
        foreach (var description in versions)
        {
            services.AddOpenApi(
                description,
                options =>
                {
                    options.AddServerFromRequest();
                    options.ApplyApiVersionInfo(document?.Title, document?.Description);
                    options.ApplySchemaNullableFalse();
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyOperationDeprecatedStatus();
                }
            );
        }
    }
}
