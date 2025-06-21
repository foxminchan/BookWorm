using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Testing;

public static class K6Extensions
{
    private const string BaseContainerPath = "Container/k6";
    private const string K6WebDashboard = "K6_WEB_DASHBOARD";
    private const string K6WebDashboardExport = "K6_WEB_DASHBOARD_EXPORT";

    /// <summary>
    ///     Adds K6 load testing to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure.</param>
    /// <param name="entryPoint">The resource builder for the project resource to test.</param>
    /// <param name="vus">Virtual Users (VUs) to simulate during the load test. Default is 10.</param>
    /// <remarks>
    ///     This method configures a K6 load testing instance that:
    ///     - Mounts scripts from a Container/scripts directory
    ///     - Mounts reports to Container/reports directory
    ///     - Runs the main.js script with a random number of virtual users (10-100)
    ///     - References and waits for the specified entryPoint resource
    ///     - Only runs in run mode, not in publish mode
    /// </remarks>
    public static void AddK6(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<YarpResource> entryPoint,
        int vus = 10
    )
    {
        var endpointName = builder.IsHttpsLaunchProfile() ? Protocol.Https : Protocol.Http;

        builder
            .AddK6(Components.K6)
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithBindMount($"{BaseContainerPath}", "/scripts", true)
            .WithBindMount($"{BaseContainerPath}/dist", "/home/k6")
            .WithScript("/scripts/dist/main.js", vus)
            .WithReference(entryPoint.Resource.GetEndpoint(endpointName))
            .WithEnvironment(K6WebDashboard, "true")
            .WithEnvironment(K6WebDashboardExport, "dashboard-report.html")
            .WithHttpEndpoint(
                targetPort: K6DashboardDefaults.ContainerPort,
                name: K6DashboardDefaults.Name
            )
            .WithUrlForEndpoint(K6DashboardDefaults.Name, url => url.DisplayText = "K6 Dashboard")
            .WithK6OtlpEnvironment()
            .WaitFor(entryPoint);
    }

    private static class K6DashboardDefaults
    {
        public const string Name = "k6-dashboard";
        public const int ContainerPort = 5665;
    }
}
