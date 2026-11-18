using Aspire.Hosting.Dotnet;
using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Network;

internal static class CorsExtensions
{
    private const string CorsSection = "Cors";
    private const string AnyHeader = "*";

    private static readonly string[] AllowedMethods =
    [
        HttpMethod.Get.Method,
        HttpMethod.Post.Method,
        HttpMethod.Put.Method,
        HttpMethod.Patch.Method,
        HttpMethod.Delete.Method,
    ];

    extension(IResourceBuilder<DotnetProjectResource> builder)
    {
        /// <summary>
        ///     Injects the <c>Cors</c> configuration section consumed by
        ///     <c>BookWorm.ServiceDefaults.Cors.CorsExtensions.AddDefaultCors</c> in non-development environments,
        ///     allowing the browser-facing origin of the API gateway.
        /// </summary>
        /// <param name="gateway">The YARP gateway that serves the frontends and proxies API requests.</param>
        /// <returns>The resource builder for method chaining.</returns>
        /// <remarks>
        ///     Uses the Aspire <c>PrimaryBrowserOrigin</c> deferred value, which resolves to the origin the browser
        ///     actually uses: <c>*.dev.localhost</c> or Codespaces URLs during local runs, and the public deployed
        ///     origin when publishing to Azure Container Apps. This removes the need to hard-code allowed origins.
        /// </remarks>
        public IResourceBuilder<DotnetProjectResource> WithGatewayCors(
            IResourceBuilder<YarpResource> gateway
        )
        {
            builder
                .WithEnvironment(
                    $"{CorsSection}__Origins__0",
                    gateway.GetEndpoint(Uri.UriSchemeHttps).PrimaryBrowserOrigin
                )
                .WithEnvironment($"{CorsSection}__Headers__0", AnyHeader)
                .WithEnvironment($"{CorsSection}__AllowCredentials", bool.TrueString);

            for (var i = 0; i < AllowedMethods.Length; i++)
            {
                builder.WithEnvironment($"{CorsSection}__Methods__{i}", AllowedMethods[i]);
            }

            return builder;
        }
    }
}
