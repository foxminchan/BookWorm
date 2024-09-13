using Aspire.Hosting.Lifecycle;

namespace BookWorm.HealthCheck.Hosting;

/// <summary>
///     https://github.com/dotnet/aspire-samples/blob/main/samples/HealthChecksUI/HealthChecksUI.AppHost/HealthChecksUIExtensions.cs
/// </summary>
public static class HealthChecksUiExtensions
{
    public static IResourceBuilder<HealthChecksUiResource> AddHealthChecksUi(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null)
    {
        builder.Services.TryAddLifecycleHook<HealthChecksUiLifecycleHook>();

        var resource = new HealthChecksUiResource(name);

        return builder
            .AddResource(resource)
            .WithImage(HealthChecksUiDefaults.ContainerImageName, HealthChecksUiDefaults.ContainerImageTag)
            .WithImageRegistry(HealthChecksUiDefaults.ContainerRegistry)
            .WithEnvironment(HealthChecksUiResource.KnownEnvVars.UiPath, "/")
            .WithHttpEndpoint(port, HealthChecksUiDefaults.ContainerPort);
    }

    public static IResourceBuilder<HealthChecksUiResource> WithReference(
        this IResourceBuilder<HealthChecksUiResource> builder,
        IResourceBuilder<ProjectResource> project,
        string endpointName = HealthChecksUiDefaults.EndpointName,
        string probePath = HealthChecksUiDefaults.ProbePath)
    {
        var monitoredProject = new MonitoredProject(project, endpointName, probePath);
        builder.Resource.MonitoredProjects.Add(monitoredProject);

        return builder;
    }
}

public static class HealthChecksUiDefaults
{
    public const string ContainerRegistry = "docker.io";

    public const string ContainerImageName = "xabarilcoding/healthchecksui";

    public const string ContainerImageTag = "5.0.0";

    public const int ContainerPort = 80;

    public const string ProbePath = "/healthz";

    public const string EndpointName = "healthchecks";
}
