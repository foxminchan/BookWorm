using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Testing;

public static class K6Extensions
{
    private const string BaseContainerPath = "Container/k6";
    private const string K6WebDashboard = "K6_WEB_DASHBOARD";
    private const string K6WebDashboardExport = "K6_WEB_DASHBOARD_EXPORT";

    /// <summary>
    ///     Adds K6 load testing to the distributed application with comprehensive performance testing capabilities.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure with K6 load testing.</param>
    /// <param name="entryPoint">The resource builder for the YARP proxy resource to test against.</param>
    /// <param name="vus">Virtual Users (VUs) to simulate during the load test. Default is 10.</param>
    public static void AddK6(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> entryPoint,
        int vus = 10
    )
    {
        builder
            .AddK6(Components.K6)
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithBindMount($"{BaseContainerPath}", "/scripts", true)
            .WithBindMount($"{BaseContainerPath}/dist", "/home/k6")
            .WithScript("/scripts/dist/main.js", vus)
            .WithReference(entryPoint.Resource.GetEndpoint(Protocol.Http))
            .WithEnvironment(K6WebDashboard, "true")
            .WithEnvironment(K6WebDashboardExport, "dashboard-report.html")
            .WithHttpEndpoint(
                targetPort: K6DashboardDefaults.ContainerPort,
                name: K6DashboardDefaults.Name
            )
            .WithUrlForEndpoint(K6DashboardDefaults.Name, url => url.DisplayText = "K6 Dashboard")
            .WithK6OtlpEnvironment()
            .WaitFor(entryPoint)
            .WithExplicitStart();
    }

    private static class K6DashboardDefaults
    {
        public const string Name = "k6-dashboard";
        public const int ContainerPort = 5665;
    }
}
