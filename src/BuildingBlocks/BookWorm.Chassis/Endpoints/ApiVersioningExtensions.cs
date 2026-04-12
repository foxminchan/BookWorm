using Asp.Versioning;
using BookWorm.Constants.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Endpoints;

public static class Extension
{
    extension(IServiceCollection service)
    {
        /// <summary>
        ///     Configures API versioning and API explorer metadata for endpoint discovery.
        /// </summary>
        /// <remarks>
        ///     Sets the default API version to <c>v1</c>, reads version values from URL segments,
        ///     and enables version substitution in route templates for grouped API documentation.
        /// </remarks>
        public void AddVersioning()
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
}
