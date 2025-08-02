﻿using Aspire.Hosting.Lifecycle;
using BookWorm.Constants.Aspire;

namespace BookWorm.HealthChecksUI;

public static class HealthChecksUIExtensions
{
    /// <summary>
    ///     Adds a HealthChecksUI container to the application model.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="port">The host port to expose the container on.</param>
    /// <param name="tag">The tag to use for the container image. Defaults to <c>"5.0.0"</c>.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<HealthChecksUIResource> AddHealthChecksUI(
        this IDistributedApplicationBuilder builder,
        string name = "health-checks-ui",
        int? port = null,
        string tag = HealthChecksUIDefaults.ContainerImageTag
    )
    {
        builder.Services.TryAddLifecycleHook<HealthChecksUILifecycleHook>();

        var resource = new HealthChecksUIResource(name);

        return builder
            .AddResource(resource)
            .WithImage(HealthChecksUIDefaults.ContainerImageName, tag)
            .WithImageRegistry(HealthChecksUIDefaults.ContainerRegistry)
            .WithEnvironment(HealthChecksUIResource.KnownEnvVars.UIPath, "/")
            .WithHttpEndpoint(port, HealthChecksUIDefaults.ContainerPort)
            .WithUrlForEndpoint(Protocols.Http, url => url.DisplayText = "Health Checks UI");
    }

    /// <summary>
    ///     Adds a reference to a project that will be monitored by the HealthChecksUI container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="project">The project.</param>
    /// <param name="endpointName">
    ///     The name of the HTTP endpoint the <see cref="ProjectResource" /> serves health check details from.
    ///     The endpoint will be added if it is not already defined on the <see cref="ProjectResource" />.
    /// </param>
    /// <param name="probePath">The request path the project serves health check details from.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<HealthChecksUIResource> WithReference(
        this IResourceBuilder<HealthChecksUIResource> builder,
        IResourceBuilder<ProjectResource> project,
        string endpointName = HealthChecksUIDefaults.EndpointName,
        string probePath = HealthChecksUIDefaults.ProbePath
    )
    {
        var monitoredProject = new MonitoredProject(project, endpointName, probePath);
        builder.Resource.MonitoredProjects.Add(monitoredProject);

        return builder;
    }

    /// <summary>
    ///     Configures a storage provider for the HealthChecksUI container.
    /// </summary>
    /// <param name="builder">The HealthChecksUI resource builder.</param>
    /// <param name="source">The resource that provides a connection string.</param>
    /// <param name="storageProvider">
    ///     The storage provider types to use.
    ///     Available options are: SqlServer, Sqlite, PostgreSql, MySql and InMemory.
    ///     Default to "PostgreSql".
    /// </param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<HealthChecksUIResource> WithStorageProvider(
        this IResourceBuilder<HealthChecksUIResource> builder,
        IResourceBuilder<IResourceWithConnectionString> source,
        string storageProvider = "PostgreSql"
    )
    {
        builder.WaitFor(source);
        builder.WithEnvironment("storage_provider", storageProvider);
        builder.WithEnvironment("storage_connection", source);
        return builder;
    }
}

/// <summary>
///     Default values used by <see cref="HealthChecksUIResource" />
/// </summary>
public static class HealthChecksUIDefaults
{
    /// <summary>
    ///     The default container registry to pull the HealthChecksUI container image from.
    /// </summary>
    public const string ContainerRegistry = "docker.io";

    /// <summary>
    ///     The default container image name to use for the HealthChecksUI container.
    /// </summary>
    public const string ContainerImageName = "xabarilcoding/healthchecksui";

    /// <summary>
    ///     The default container image tag to use for the HealthChecksUI container.
    /// </summary>
    public const string ContainerImageTag = "5.0.0";

    /// <summary>
    ///     The target port the HealthChecksUI container listens on.
    /// </summary>
    public const int ContainerPort = 80;

    /// <summary>
    ///     The default request path projects serve health check details from.
    /// </summary>
    public const string ProbePath = "/healthz";

    /// <summary>
    ///     The default name of the HTTP endpoint projects serves health check details from.
    /// </summary>
    public const string EndpointName = "healthchecks";
}
