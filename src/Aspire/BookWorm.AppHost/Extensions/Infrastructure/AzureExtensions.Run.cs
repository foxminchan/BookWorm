namespace BookWorm.AppHost.Extensions.Infrastructure;

/// <summary>
///     Provides extension methods for configuring Azure resources to run as local container emulators
///     during development. This partial class handles container-based emulation of Azure services
///     with development-friendly configurations and integrated management tools.
/// </summary>
/// <remarks>
///     <para>
///         This class enables cost-effective local development by running Azure service emulators
///         in containers instead of consuming cloud resources. Supported services include:
///         - Azure Storage (Azurite emulator) with persistent data volumes
///         - Azure SignalR (local emulator) with persistent lifetime
///         - PostgreSQL (containerized) with PgWeb administration interface
///         - Redis (containerized) with RedisInsight and clear cache commands
///     </para>
///     <para>
///         All container configurations include:
///         - Persistent data volumes for data retention across restarts
///         - Always pull latest images for up-to-date emulator versions
///         - Persistent container lifetime for development session continuity
///         - Integrated web-based management tools for debugging and monitoring
///     </para>
///     <para>
///         These methods are execution-context aware and only apply container configurations
///         during local development (run mode). In publishing mode, they configure appropriate
///         authentication mechanisms for cloud deployment.
///     </para>
/// </remarks>
public static partial class AzureExtensions
{
    /// <summary>
    ///     Configures the Azure Storage resource to run as a container in the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Storage.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureStorageResource> RunAsContainer(
        this IResourceBuilder<AzureStorageResource> builder
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.RunAsEmulator(config =>
                config
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );
        }

        return builder;
    }

    /// <summary>
    ///     Configures the Azure SignalR resource to run as a container in the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure SignalR.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureSignalRResource> RunAsContainer(
        this IResourceBuilder<AzureSignalRResource> builder
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.RunAsEmulator(config =>
                config
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );
        }

        return builder;
    }

    /// <summary>
    ///     Configures the Azure PostgreSQL Flexible Server resource to run as a container or use password authentication based
    ///     on the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure PostgreSQL Flexible Server.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzurePostgresFlexibleServerResource> RunAsContainer(
        this IResourceBuilder<AzurePostgresFlexibleServerResource> builder
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.RunAsContainer(cfg =>
                cfg.WithPgWeb()
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );
        }
        else
        {
            builder.WithPasswordAuthentication();
        }

        return builder;
    }

    /// <summary>
    ///     Configures the Azure Redis Cache resource to run as a container or use access key authentication based on the
    ///     execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Redis Cache.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureRedisCacheResource> RunAsContainer(
        this IResourceBuilder<AzureRedisCacheResource> builder
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.RunAsContainer(config =>
                config
                    .WithDataVolume()
                    .WithClearCommand()
                    .WithRedisInsight()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );
        }
        else
        {
            builder.WithAccessKeyAuthentication();
        }

        return builder;
    }
}
