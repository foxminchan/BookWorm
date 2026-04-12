using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Testing;

internal static class K6Extensions
{
    private const string BaseContainerPath = "Container/k6";
    private const string K6WebDashboard = "K6_WEB_DASHBOARD";
    private const string K6WebDashboardExport = "K6_WEB_DASHBOARD_EXPORT";

    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds K6 load testing to the distributed application with comprehensive performance testing capabilities.
        /// </summary>
        /// <param name="entryPoint">The resource builder for the YARP proxy resource to test against.</param>
        /// <param name="vus">Virtual Users (VUs) to simulate during the load test. Default is 10.</param>
        public void AddK6(IResourceBuilder<YarpResource> entryPoint, int vus = 10)
        {
            var scriptPath = Path.GetFullPath($"{BaseContainerPath}", builder.AppHostDirectory);
            var distPath = Path.GetFullPath($"{BaseContainerPath}/dist", builder.AppHostDirectory);

            builder
                .AddK6(Components.K6)
                .WithIconName("BeakerAdd")
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithBindMount($"{scriptPath}", "/scripts", true)
                .WithBindMount($"{distPath}", "/home/k6")
                .WithScript("/scripts/dist/main.js", vus)
                .WithReference(entryPoint.Resource.GetEndpoint(Uri.UriSchemeHttp))
                .WithEnvironment(K6WebDashboard, "true")
                .WithEnvironment(K6WebDashboardExport, "dashboard-report.html")
                .WithHttpEndpoint(
                    targetPort: K6DashboardDefaults.ContainerPort,
                    name: K6DashboardDefaults.Name
                )
                .WithUrlForEndpoint(
                    K6DashboardDefaults.Name,
                    url => url.DisplayText = "K6 Dashboard"
                )
                .WithK6OtlpEnvironment()
                .WaitFor(entryPoint)
                .WithExplicitStart();
        }
    }

    private static class K6DashboardDefaults
    {
        public const string Name = "k6-dashboard";
        public const int ContainerPort = 5665;
    }
}
