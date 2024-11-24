using Asp.Versioning;

namespace BookWorm.Shared.Versioning;

public static class Extension
{
    public static IHostApplicationBuilder AddVersioning(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new(1, 0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        return builder;
    }
}
