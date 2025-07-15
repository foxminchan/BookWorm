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
    /// <remarks>
    ///     This method configures a comprehensive K6 load testing environment with the following features:
    ///     - <strong>Script management:</strong> Mounts test scripts from <c>Container/k6</c> directory for easy script
    ///     development
    ///     - <strong>Report generation:</strong> Mounts output directory to <c>Container/k6/dist</c> for persistent test
    ///     reports
    ///     - <strong>Main script execution:</strong> Runs <c>/scripts/dist/main.js</c> with configurable virtual user count
    ///     - <strong>Target configuration:</strong> References and waits for the specified YARP entry point resource
    ///     - <strong>Web dashboard:</strong> Enables K6 web dashboard on port 5665 for real-time test monitoring
    ///     - <strong>Dashboard export:</strong> Automatically exports dashboard reports as <c>dashboard-report.html</c>
    ///     - <strong>OpenTelemetry integration:</strong> Configures OTLP environment for distributed tracing
    ///     - <strong>Container management:</strong> Uses always pull policy for latest K6 features and bug fixes
    ///     - <strong>Dependency management:</strong> Ensures proper startup order by waiting for entry point availability
    /// </remarks>
    /// <example>
    ///     <code>
    ///     // Basic load testing with default 10 virtual users
    ///     var yarp = builder.AddYarp("gateway");
    ///     builder.AddK6(yarp);
    ///
    ///     // Custom virtual user count for stress testing
    ///     builder.AddK6(yarp, vus: 50);
    ///     </code>
    /// </example>
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
            .WithReference(entryPoint.Resource.GetEndpoint(builder.GetLaunchProfileName()))
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
