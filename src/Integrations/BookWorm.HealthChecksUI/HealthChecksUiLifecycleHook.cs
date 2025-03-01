using System.Diagnostics;
using Aspire.Hosting.Lifecycle;

namespace BookWorm.HealthChecksUI;

internal sealed class HealthChecksUiLifecycleHook(
    DistributedApplicationExecutionContext executionContext
) : IDistributedApplicationLifecycleHook
{
    private const string HealthchecksuiUrls = "HEALTHCHECKSUI_URLS";

    public Task BeforeStartAsync(
        DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default
    )
    {
        // Configure each project referenced by a Health Checks UI resource
        var healthChecksUiResources = appModel.Resources.OfType<HealthChecksUiResource>();

        foreach (var healthChecksUiResource in healthChecksUiResources)
        {
            foreach (var monitoredProject in healthChecksUiResource.MonitoredProjects)
            {
                var project = monitoredProject.Project;

                // Add the health check endpoint if it doesn't exist
                var healthChecksEndpoint = project.GetEndpoint(monitoredProject.EndpointName);
                if (!healthChecksEndpoint.Exists)
                {
                    project.WithHttpEndpoint(name: monitoredProject.EndpointName);
                    Debug.Assert(
                        healthChecksEndpoint.Exists,
                        "The health check endpoint should exist after adding it."
                    );
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
                        var containerHost = healthChecksUiResource
                            .GetEndpoint("http")
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
                        HealthchecksuiUrls,
                        healthChecksEndpointsExpression
                    );
                });
            }
        }

        if (executionContext.IsPublishMode)
        {
            ConfigureHealthChecksUiContainers(appModel.Resources, true);
        }

        return Task.CompletedTask;
    }

    public Task AfterEndpointsAllocatedAsync(
        DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default
    )
    {
        ConfigureHealthChecksUiContainers(appModel.Resources, false);

        return Task.CompletedTask;
    }

    private static void ConfigureHealthChecksUiContainers(
        IResourceCollection resources,
        bool isPublishing
    )
    {
        var healthChecksUiResources = resources.OfType<HealthChecksUiResource>();

        foreach (var healthChecksUiResource in healthChecksUiResources)
        {
            var monitoredProjects = healthChecksUiResource.MonitoredProjects;

            // Add environment variables to configure the HealthChecksUI container with the health checks endpoints of each referenced project
            // See example configuration at https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#sample-2-configuration-using-appsettingsjson
            for (var i = 0; i < monitoredProjects.Count; i++)
            {
                var monitoredProject = monitoredProjects[i];
                var healthChecksEndpoint = monitoredProject.Project.GetEndpoint(
                    monitoredProject.EndpointName
                );

                // Set health check name
                var nameEnvVarName = HealthChecksUiResource.KnownEnvVars.GetHealthCheckNameKey(i);
                healthChecksUiResource.Annotations.Add(
                    new EnvironmentCallbackAnnotation(nameEnvVarName, () => monitoredProject.Name)
                );

                // Set health check URL
                var probePath = monitoredProject.ProbePath.TrimStart('/');
                var urlEnvVarName = HealthChecksUiResource.KnownEnvVars.GetHealthCheckUriKey(i);

                healthChecksUiResource.Annotations.Add(
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
