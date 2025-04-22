using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Saunter;

namespace BookWorm.ServiceDefaults.ApiSpecification.AsyncApi;

public static class AsyncApiExtensions
{
    public static void AddDefaultAsyncApi(this IHostApplicationBuilder builder, IList<Type> types)
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
                            Email = "nguyenxuannhan407@gmail.com",
                        },
                    },
                };
            }
        });
    }

    public static void UseDefaultAsyncApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapAsyncApiDocuments();
        app.MapAsyncApiUi();
    }
}
