using Aspire.Hosting.Lifecycle;
using BookWorm.Constants.Aspire;

namespace BookWorm.HealthChecksUI;

internal sealed class HealthChecksUILifecycleHook(
    DistributedApplicationExecutionContext executionContext
) : IDistributedApplicationLifecycleHook
{
    private const string HealthChecksUIUrls = "HEALTHCHECKSUI_URLS";

    public Task BeforeStartAsync(
        DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default
    )
    {
        // Configure each project referenced by a Health Checks UI resource
        var healthChecksUIResources = appModel.Resources.OfType<HealthChecksUIResource>();

        foreach (var healthChecksUIResource in healthChecksUIResources)
        {
            foreach (var monitoredProject in healthChecksUIResource.MonitoredProjects)
            {
                var project = monitoredProject.Project;

                // Add the health check endpoint if it doesn't exist
                var healthChecksEndpoint = project.GetEndpoint(monitoredProject.EndpointName);
                if (!healthChecksEndpoint.Exists)
                {
                    project.WithHttpEndpoint(name: monitoredProject.EndpointName);
                }

                // Set environment variable to configure the URLs the health check endpoint is accessible from
                project.WithEnvironment(context =>
                {
                    var probePath = monitoredProject.ProbePath.TrimStart('/');
                    var healthChecksEndpointsExpression = ReferenceExpression.Create(
                        $"{healthChecksEndpoint}/{probePath}"
                    );

                    if (context.ExecutionContext.IsRunMode)
                    {
                        // Running during dev inner-loop
                        var containerHost = healthChecksUIResource
                            .GetEndpoint(Protocols.Http)
                            .ContainerHost;
                        var fromContainerUriBuilder = new UriBuilder(healthChecksEndpoint.Url)
                        {
                            Host = containerHost,
                            Path = monitoredProject.ProbePath,
                        };

                        healthChecksEndpointsExpression = ReferenceExpression.Create(
                            $"{healthChecksEndpointsExpression};{fromContainerUriBuilder.ToString()}"
                        );
                    }

                    context.EnvironmentVariables.Add(
                        HealthChecksUIUrls,
                        healthChecksEndpointsExpression
                    );
                });
            }
        }

        if (executionContext.IsPublishMode)
        {
            ConfigureHealthChecksUIContainers(appModel.Resources, true);
        }

        return Task.CompletedTask;
    }

    public Task AfterEndpointsAllocatedAsync(
        DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default
    )
    {
        ConfigureHealthChecksUIContainers(appModel.Resources, false);

        return Task.CompletedTask;
    }

    private static void ConfigureHealthChecksUIContainers(
        IResourceCollection resources,
        bool isPublishing
    )
    {
        var healthChecksUIResources = resources.OfType<HealthChecksUIResource>();

        foreach (var healthChecksUIResource in healthChecksUIResources)
        {
            var monitoredProjects = healthChecksUIResource.MonitoredProjects;

            // Add environment variables to configure the HealthChecksUI container with the health checks endpoints of each referenced project
            // See example configuration at https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#sample-2-configuration-using-appsettingsjson
            for (var i = 0; i < monitoredProjects.Count; i++)
            {
                var monitoredProject = monitoredProjects[i];
                var healthChecksEndpoint = monitoredProject.Project.GetEndpoint(
                    monitoredProject.EndpointName
                );

                // Set health check name
                var nameEnvVarName = HealthChecksUIResource.KnownEnvVars.GetHealthCheckNameKey(i);
                healthChecksUIResource.Annotations.Add(
                    new EnvironmentCallbackAnnotation(nameEnvVarName, () => monitoredProject.Name)
                );

                // Set health check URL
                var probePath = monitoredProject.ProbePath.TrimStart('/');
                var urlEnvVarName = HealthChecksUIResource.KnownEnvVars.GetHealthCheckUriKey(i);

                healthChecksUIResource.Annotations.Add(
                    new EnvironmentCallbackAnnotation(context =>
                        context[urlEnvVarName] = isPublishing
                            ? ReferenceExpression.Create($"{healthChecksEndpoint}/{probePath}")
                            : new HostUrl($"{healthChecksEndpoint.Url}/{probePath}")
                    )
                );
            }
        }
    }
}
