using Asp.Versioning;
using BookWorm.Constants.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Endpoints;

public static class Extension
{
    public static void AddVersioning(this IServiceCollection service)
    {
        service
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersions.V1;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
