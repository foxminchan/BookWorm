namespace BookWorm.HealthChecksUI;

/// <summary>
///     A container-based resource for the HealthChecksUI container.
///     See https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#HealthCheckUI
/// </summary>
/// <param name="name">The resource name.</param>
public class HealthChecksUIResource(string name)
    : ContainerResource(name),
        IResourceWithServiceDiscovery
{
    /// <summary>
    ///     The projects to be monitored by the HealthChecksUI container.
    /// </summary>
    public IList<MonitoredProject> MonitoredProjects { get; } = [];

    /// <summary>
    ///     Known environment variables for the HealthChecksUI container that can be used to configure the container.
    ///     Taken from
    ///     https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/doc/ui-docker.md#environment-variables-table
    /// </summary>
    public static class KnownEnvVars
    {
        public const string UIPath = "ui_path";

        // These keys are taken from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#sample-2-configuration-using-appsettingsjson
        private const string HealthChecksConfigSection = "HealthChecksUI__HealthChecks";
        private const string HealthCheckName = "Name";
        private const string HealthCheckUri = "Uri";

        internal static string GetHealthCheckNameKey(int index)
        {
            return $"{HealthChecksConfigSection}__{index}__{HealthCheckName}";
        }

        internal static string GetHealthCheckUriKey(int index)
        {
            return $"{HealthChecksConfigSection}__{index}__{HealthCheckUri}";
        }
    }
}

/// <summary>
///     Represents a project to be monitored by a <see cref="HealthChecksUIResource" />.
/// </summary>
public sealed class MonitoredProject(
    IResourceBuilder<ProjectResource> project,
    string endpointName,
    string probePath
)
{
    private string? _name;

    /// <summary>
    ///     The project to be monitored.
    /// </summary>
    public IResourceBuilder<ProjectResource> Project { get; } =
        project ?? throw new ArgumentNullException(nameof(project));

    /// <summary>
    ///     The name of the endpoint the project serves health check details from.
    ///     If it doesn't exist, it will be added.
    /// </summary>
    public string EndpointName { get; } =
        endpointName ?? throw new ArgumentNullException(nameof(endpointName));

    /// <summary>
    ///     The name of the project to be displayed in the HealthChecksUI dashboard. Defaults to the project resource's name.
    /// </summary>
    public string Name
    {
        get => _name ?? Project.Resource.Name;
        set => _name = value;
    }

    /// <summary>
    ///     The request path the project serves health check details for the HealthChecksUI dashboard from.
    /// </summary>
    public string ProbePath { get; set; } =
        probePath ?? throw new ArgumentNullException(nameof(probePath));
}
