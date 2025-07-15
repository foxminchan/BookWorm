namespace BookWorm.AppHost.Extensions.Infrastructure;

public static partial class AzureExtensions
{
    /// <summary>
    ///     Configures the Azure Storage resource to run as a container in the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Storage.</param>
    /// <returns>The updated resource builder with container configuration applied.</returns>
    /// <remarks>
    ///     This method enables local development by running Azure Storage as an Azurite emulator container:
    ///     - Only applies container configuration in run mode (local development)
    ///     - Uses Azurite emulator for blob, queue, and table storage
    ///     - Includes persistent data volume for data retention across restarts
    ///     - Always pulls latest emulator image for up-to-date features
    ///     - Sets persistent container lifetime for development session continuity
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureStorage("storage")
    ///            .RunAsContainer();
    ///     </code>
    /// </example>
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
    /// <returns>The updated resource builder with container configuration applied.</returns>
    /// <remarks>
    ///     This method enables local development by running Azure SignalR as a local emulator container:
    ///     - Only applies container configuration in run mode (local development)
    ///     - Uses SignalR emulator for real-time communication features
    ///     - Always pulls latest emulator image for up-to-date features
    ///     - Sets persistent container lifetime for development session continuity
    ///     - No data volume needed as SignalR is stateless
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureSignalR("signalr")
    ///            .RunAsContainer();
    ///     </code>
    /// </example>
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
    /// <returns>The updated resource builder with appropriate configuration for the execution context.</returns>
    /// <remarks>
    ///     This method provides dual configuration based on execution context:
    ///     - <strong>Run mode (local development):</strong> Runs PostgreSQL as a container with PgWeb admin interface,
    ///     persistent data volume, and development-friendly settings
    ///     - <strong>Publish mode (cloud deployment):</strong> Configures password authentication for cloud PostgreSQL
    ///     instance
    ///     - Includes persistent container lifetime and always pulls latest images in run mode
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzurePostgresFlexibleServer("postgres")
    ///            .RunAsContainer();
    ///     </code>
    /// </example>
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
    /// <returns>The updated resource builder with appropriate configuration for the execution context.</returns>
    /// <remarks>
    ///     This method provides dual configuration based on execution context:
    ///     - <strong>Run mode (local development):</strong> Runs Redis as a container with RedisInsight management interface,
    ///     clear cache commands, and persistent data volume
    ///     - <strong>Publish mode (cloud deployment):</strong> Configures access key authentication for cloud Redis instance
    ///     - Includes persistent container lifetime and always pulls latest images in run mode
    ///     - Provides integrated debugging tools for local development
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureRedis("redis")
    ///            .RunAsContainer();
    ///     </code>
    /// </example>
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
