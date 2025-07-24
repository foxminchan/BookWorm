using System.Security.Cryptography;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Common;

public static class DistributedApplicationExtensions
{
    public static TBuilder WithRandomParameterValues<TBuilder>(this TBuilder builder)
        where TBuilder : IDistributedApplicationTestingBuilder
    {
        var parameters = builder
            .Resources.OfType<ParameterResource>()
            .Where(p => !p.IsConnectionString)
            .ToList();

        foreach (var parameter in parameters)
        {
            builder.Configuration[$"Parameters:{parameter.Name}"] = parameter.Secret
                ? PasswordGenerator.Generate(16, true, true, true, false, 1, 1, 1, 0)
                : Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        }

        return builder;
    }

    public static TBuilder WithContainersLifetime<TBuilder>(
        this TBuilder builder,
        ContainerLifetime containerLifetime
    )
        where TBuilder : IDistributedApplicationTestingBuilder
    {
        var containerLifetimeAnnotations = builder
            .Resources.SelectMany(r =>
                r.Annotations.OfType<ContainerLifetimeAnnotation>()
                    .Where(c => c.Lifetime != containerLifetime)
            )
            .ToList();

        foreach (var annotation in containerLifetimeAnnotations)
        {
            annotation.Lifetime = containerLifetime;
        }

        return builder;
    }

    public static TBuilder WithRandomVolumeNames<TBuilder>(this TBuilder builder)
        where TBuilder : IDistributedApplicationTestingBuilder
    {
        var allResourceNamedVolumes = builder
            .Resources.SelectMany(r =>
                r.Annotations.OfType<ContainerMountAnnotation>()
                    .Where(m =>
                        m.Type == ContainerMountType.Volume && !string.IsNullOrEmpty(m.Source)
                    )
                    .Select(m => (Resource: r, Volume: m))
            )
            .ToList();

        var seenVolumes = new HashSet<string>();

        var renamedVolumes = new Dictionary<string, string>();

        foreach (
            var name in allResourceNamedVolumes
                .Select(resourceVolume => resourceVolume.Volume.Source!)
                .Where(name => !seenVolumes.Add(name) && !renamedVolumes.ContainsKey(name))
        )
        {
            renamedVolumes[name] =
                $"{name}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(4))}";
        }

        // Replace all named volumes with randomly named or anonymous volumes
        foreach (var (resource, volume) in allResourceNamedVolumes)
        {
            var newName = renamedVolumes.GetValueOrDefault(volume.Source!);
            var newMount = new ContainerMountAnnotation(
                newName,
                volume.Target,
                ContainerMountType.Volume,
                volume.IsReadOnly
            );
            resource.Annotations.Remove(volume);
            resource.Annotations.Add(newMount);
        }

        return builder;
    }

    public static Task WaitForResource(
        this DistributedApplication app,
        string resourceName,
        string? targetState = null,
        CancellationToken cancellationToken = default
    )
    {
        targetState ??= KnownResourceStates.Running;
        var resourceNotificationService =
            app.Services.GetRequiredService<ResourceNotificationService>();

        return resourceNotificationService.WaitForResourceAsync(
            resourceName,
            targetState,
            cancellationToken
        );
    }

    public static async Task WaitForResourcesAsync(
        this DistributedApplication app,
        IEnumerable<string>? targetStates = null,
        CancellationToken cancellationToken = default
    )
    {
        var services = app.Services;
        var logger = services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger($"{nameof(BookWorm)}.{nameof(WaitForResourcesAsync)}");

        targetStates ??= [KnownResourceStates.Running, .. KnownResourceStates.TerminalStates];

        var applicationModel = services.GetRequiredService<DistributedApplicationModel>();

        var resourceTasks = new Dictionary<string, Task<(string Name, string State)>>();

        var states = targetStates as string[] ?? [.. targetStates];

        foreach (var resource in applicationModel.Resources)
        {
            if (resource is IResourceWithoutLifetime)
            {
                continue;
            }
            resourceTasks[resource.Name] = GetResourceWaitTask(
                resource.Name,
                states,
                cancellationToken
            );
        }

        logger.LogInformation(
            "Waiting for resources [{Resources}] to reach one of target states [{TargetStates}].",
            string.Join(',', resourceTasks.Keys),
            string.Join(',', states)
        );

        while (resourceTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(resourceTasks.Values);
            var (completedResourceName, targetStateReached) = await completedTask;

            if (targetStateReached == KnownResourceStates.FailedToStart)
            {
                throw new DistributedApplicationException(
                    $"Resource '{completedResourceName}' failed to start."
                );
            }

            resourceTasks.Remove(completedResourceName);

            logger.LogInformation(
                "Wait for resource '{ResourceName}' completed with state '{ResourceState}'",
                completedResourceName,
                targetStateReached
            );

            // Ensure resources being waited on still exist
            var remainingResources = resourceTasks.Keys.ToList();
            for (var i = remainingResources.Count - 1; i > 0; i--)
            {
                var name = remainingResources[i];
                if (applicationModel.Resources.Any(r => r.Name == name))
                {
                    continue;
                }

                logger.LogInformation(
                    "Resource '{ResourceName}' was deleted while waiting for it.",
                    name
                );
                resourceTasks.Remove(name);
                remainingResources.RemoveAt(i);
            }

            if (resourceTasks.Count > 0)
            {
                logger.LogInformation(
                    "Still waiting for resources [{Resources}] to reach one of target states [{TargetStates}].",
                    string.Join(',', remainingResources),
                    string.Join(',', states)
                );
            }
        }

        logger.LogInformation("Wait for all resources completed successfully!");
        return;

        async Task<(string Name, string State)> GetResourceWaitTask(
            string resourceName,
            IEnumerable<string> resourceStates,
            CancellationToken ctx
        )
        {
            var state = await app.ResourceNotifications.WaitForResourceAsync(
                resourceName,
                resourceStates,
                ctx
            );
            return (resourceName, state);
        }
    }
}
