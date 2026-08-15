using Asp.Versioning;
using BookWorm.Constants.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Endpoints;

public static class Extension
{
    private const string ApiVersionHeaderName = "api-version";

    extension(IServiceCollection service)
    {
        /// <summary>
        ///     Configures API versioning and API explorer metadata for endpoint discovery.
        /// </summary>
        /// <remarks>
        ///     Sets the default API version to <c>v1</c>, reads REST versions from URL segments
        ///     and gRPC versions from metadata, and enables version substitution in route templates
        ///     for grouped API documentation.
        /// </remarks>
        public void AddVersioning()
        {
            service
                .AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = ApiVersions.V1;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader(ApiVersionHeaderName)
                    );
                    options.ReportApiVersions = true;
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'V";
                    options.SubstituteApiVersionInUrl = true;
                    options.DefaultApiVersionParameterDescription =
                        "The API version, in the format 'vX' where X is the version number (e.g. v1)";
                });
        }
    }
}
