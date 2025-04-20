using Aspire.Hosting.Azure;

namespace BookWorm.AppHost.Extensions;

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
                    .WithRedisInsight()
                    .WithDataVolume()
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
