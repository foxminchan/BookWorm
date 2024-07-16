using System.Diagnostics;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace BookWorm.HealthCheck.Hosting;

public sealed class HealthChecksUiResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    public IList<MonitoredProject> MonitoredProjects { get; } = [];

    public static class KnownEnvVars
    {
        public const string UiPath = "ui_path";
        public const string HealthChecksConfigSection = "HealthChecksUI__HealthChecks";
        public const string HealthCheckName = "Name";
        public const string HealthCheckUri = "Uri";

        internal static string GetHealthCheckNameKey(int index) =>
            $"{HealthChecksConfigSection}__{index}__{HealthCheckName}";

        internal static string GetHealthCheckUriKey(int index) =>
            $"{HealthChecksConfigSection}__{index}__{HealthCheckUri}";
    }
}

public sealed class MonitoredProject(IResourceBuilder<ProjectResource> project, string endpointName, string probePath)
{
    private string? _name;

    public IResourceBuilder<ProjectResource> Project { get; } =
        project ?? throw new ArgumentNullException(nameof(project));

    public string EndpointName { get; } = endpointName ?? throw new ArgumentNullException(nameof(endpointName));

    public string Name
    {
        get => _name ?? Project.Resource.Name;
        set { _name = value; }
    }

    public string ProbePath { get; set; } = probePath ?? throw new ArgumentNullException(nameof(probePath));
}

internal sealed class HealthChecksUiLifecycleHook(DistributedApplicationExecutionContext executionContext)
    : IDistributedApplicationLifecycleHook
{
    private const int DefaultAspNetCoreContainerPort = 8080;
    private const int DefaultHealthChecksPort = 8081;
    private const string HealthChecksUiUrls = "HEALTHCHECKSUI_URLS";
    private const string AspnetcoreHttpPorts = "ASPNETCORE_HTTP_PORTS";
    private const string AspnetcoreHttpsPorts = "ASPNETCORE_HTTPS_PORTS";

    public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        var healthChecksUiResources = appModel.Resources.OfType<HealthChecksUiResource>();

        foreach (var healthChecksUiResource in healthChecksUiResources)
        {
            foreach (var monitoredProject in healthChecksUiResource.MonitoredProjects)
            {
                var project = monitoredProject.Project;

                var healthChecksEndpoint = project.GetEndpoint(monitoredProject.EndpointName);
                if (!healthChecksEndpoint.Exists)
                {
                    int? targetPort = executionContext.IsPublishMode ? DefaultHealthChecksPort : null;
                    project.WithHttpEndpoint(targetPort: targetPort, name: monitoredProject.EndpointName);
                    Debug.Assert(healthChecksEndpoint.Exists,
                        "The health check endpoint should exist after adding it.");
                }

                project.WithEnvironment(context =>
                {
                    var probePath = monitoredProject.ProbePath.TrimStart('/');
                    var healthChecksEndpointsExpression =
                        ReferenceExpression.Create($"{healthChecksEndpoint}/{probePath}");

                    if (context.ExecutionContext.IsRunMode)
                    {
                        var containerHost = healthChecksUiResource.GetEndpoint("http").ContainerHost;
                        var fromContainerUriBuilder = new UriBuilder(healthChecksEndpoint.Url)
                        {
                            Host = containerHost, Path = monitoredProject.ProbePath
                        };

                        healthChecksEndpointsExpression =
                            ReferenceExpression.Create(
                                $"{healthChecksEndpointsExpression};{fromContainerUriBuilder.ToString()}");
                    }
                    else if (context.ExecutionContext.IsPublishMode)
                    {
                        if (!context.EnvironmentVariables.ContainsKey(AspnetcoreHttpPorts)
                            && healthChecksEndpoint is { Scheme: "http", TargetPort: { } httpPort } &&
                            httpPort != DefaultAspNetCoreContainerPort)
                        {
                            context.EnvironmentVariables.Add(
                                AspnetcoreHttpPorts,
                                ReferenceExpression.Create(
                                    $"{DefaultAspNetCoreContainerPort.ToString()};{healthChecksEndpoint.Property(EndpointProperty.TargetPort)}"));
                        }

                        if (!context.EnvironmentVariables.ContainsKey(AspnetcoreHttpsPorts)
                            && healthChecksEndpoint is { Scheme: "https", TargetPort: not null })
                        {
                            context.EnvironmentVariables.Add(
                                AspnetcoreHttpsPorts,
                                ReferenceExpression.Create(
                                    $"{healthChecksEndpoint.Property(EndpointProperty.TargetPort)}"));
                        }
                    }

                    context.EnvironmentVariables.Add(HealthChecksUiUrls, healthChecksEndpointsExpression);
                });
            }
        }

        if (executionContext.IsPublishMode)
        {
            ConfigureHealthChecksUiContainers(appModel.Resources, isPublishing: true);
        }

        return Task.CompletedTask;
    }

    public static Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default)
    {
        ConfigureHealthChecksUiContainers(appModel.Resources, isPublishing: false);

        return Task.CompletedTask;
    }

    private static void ConfigureHealthChecksUiContainers(IResourceCollection resources, bool isPublishing)
    {
        var healthChecksUiResources = resources.OfType<HealthChecksUiResource>();

        foreach (var healthChecksUiResource in healthChecksUiResources)
        {
            var monitoredProjects = healthChecksUiResource.MonitoredProjects;

            for (var i = 0; i < monitoredProjects.Count; i++)
            {
                var monitoredProject = monitoredProjects[i];
                var healthChecksEndpoint = monitoredProject.Project.GetEndpoint(monitoredProject.EndpointName);
                var nameEnvVarName = HealthChecksUiResource.KnownEnvVars.GetHealthCheckNameKey(i);
                healthChecksUiResource.Annotations.Add(
                    new EnvironmentCallbackAnnotation(
                        nameEnvVarName,
                        () => monitoredProject.Name));
                var probePath = monitoredProject.ProbePath.TrimStart('/');
                var urlEnvVarName = HealthChecksUiResource.KnownEnvVars.GetHealthCheckUriKey(i);

                healthChecksUiResource.Annotations.Add(
                    new EnvironmentCallbackAnnotation(
                        context => context[urlEnvVarName] = isPublishing
                            ? ReferenceExpression.Create($"{healthChecksEndpoint}/{probePath}")
                            : new HostUrl($"{healthChecksEndpoint.Url}/{probePath}")));
            }
        }
    }
}
