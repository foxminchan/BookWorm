using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Saunter;

namespace BookWorm.ServiceDefaults.ApiSpecification.AsyncApi;

public static class AsyncApiExtensions
{
    public static void AddAsyncApiDocs(this IHostApplicationBuilder builder, IList<Type> types)
    {
        var services = builder.Services;

        var document = builder.Configuration.GetSection(nameof(Document)).Get<Document>();

        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.AssemblyMarkerTypes = types;

            string[] versions = ["1.0.0"];

            foreach (var version in versions)
            {
                options.AsyncApi = new()
                {
                    Info = new(document?.Title, version)
                    {
                        License = new("MIT") { Url = new("https://opensource.org/licenses/MIT") },
                        Contact = new()
                        {
                            Name = "Nhan Nguyen",
                            Url = new("https://github.com/foxminchan"),
                        },
                    },
                };
            }
        });
    }

    public static void MapAsyncApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAsyncApiDocuments();
        endpoints.MapAsyncApiUi();
    }
}
