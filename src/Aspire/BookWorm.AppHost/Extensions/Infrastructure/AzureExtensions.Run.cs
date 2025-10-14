namespace BookWorm.AppHost.Extensions.Infrastructure;

public static partial class AzureExtensions
{
    /// <summary>
    ///     Configures the Azure Storage resource to run as a container in the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Storage.</param>
    /// <returns>The updated resource builder with container configuration applied.</returns>
    public static IResourceBuilder<AzureStorageResource> RunAsLocalContainer(
        this IResourceBuilder<AzureStorageResource> builder
    )
    {
        builder.RunAsEmulator(config =>
            config
                .WithDataVolume()
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithLifetime(ContainerLifetime.Persistent)
        );

        return builder;
    }

    /// <summary>
    ///     Configures the Azure SignalR resource to run as a container in the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure SignalR.</param>
    /// <returns>The updated resource builder with container configuration applied.</returns>
    public static IResourceBuilder<AzureSignalRResource> RunAsLocalContainer(
        this IResourceBuilder<AzureSignalRResource> builder
    )
    {
        builder.RunAsEmulator(config =>
            config
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithLifetime(ContainerLifetime.Persistent)
        );

        return builder;
    }

    /// <summary>
    ///     Configures the Azure PostgreSQL Flexible Server resource to run as a container or use password authentication based
    ///     on the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure PostgreSQL Flexible Server.</param>
    /// <returns>The updated resource builder with appropriate configuration for the execution context.</returns>
    public static IResourceBuilder<AzurePostgresFlexibleServerResource> RunAsLocalContainer(
        this IResourceBuilder<AzurePostgresFlexibleServerResource> builder
    )
    {
        builder.RunAsContainer(cfg =>
        {
            cfg.WithPgAdmin()
                .WithDataVolume()
                .WithImageTag("18.0")
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithLifetime(ContainerLifetime.Persistent);
        });

        return builder;
    }

    /// <summary>
    ///     Configures the Azure Redis Cache resource to run as a container or use access key authentication based on the
    ///     execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Redis Cache.</param>
    /// <returns>The updated resource builder with appropriate configuration for the execution context.</returns>
    public static IResourceBuilder<AzureRedisCacheResource> RunAsLocalContainer(
        this IResourceBuilder<AzureRedisCacheResource> builder
    )
    {
        builder.RunAsContainer(config =>
            config
                .WithIconName("memory")
                .WithDataVolume()
                .WithRedisInsight()
                .WithImagePullPolicy(ImagePullPolicy.Always)
                .WithLifetime(ContainerLifetime.Persistent)
        );

        return builder;
    }
}
