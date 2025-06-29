using Saunter;

namespace BookWorm.ServiceDefaults.ApiSpecification.AsyncApi;

public static class AsyncApiExtensions
{
    public static void AddDefaultAsyncApi(this IHostApplicationBuilder builder, IList<Type> types)
    {
        var services = builder.Services;

        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.AssemblyMarkerTypes = types;

            using var sp = services.BuildServiceProvider();
            var document = sp.GetRequiredService<DocumentOptions>();

            foreach (var version in sp.GetApiVersionDescription())
            {
                options.AsyncApi = new()
                {
                    Info = new(document.Title, version.ApiVersion.ToString())
                    {
                        Description = ApiVersionDescriptionBuilder.BuildDescription(
                            version,
                            document.Description
                        ),
                        License = new(document.LicenseName)
                        {
                            Url = document.LicenseUrl.ToString(),
                        },
                        Contact = new()
                        {
                            Name = document.AuthorName,
                            Url = document.AuthorUrl.ToString(),
                            Email = document.AuthorEmail,
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
