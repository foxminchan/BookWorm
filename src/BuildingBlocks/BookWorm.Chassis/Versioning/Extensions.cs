using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Versioning;

public static class Extension
{
    public static void AddVersioning(this IServiceCollection service)
    {
        service
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new(1, 0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
