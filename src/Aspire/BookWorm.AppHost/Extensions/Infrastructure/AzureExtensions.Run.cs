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
                .WithArgs("--disableProductStyleUrl")
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
                // Issue: https://github.com/dotnet/aspire/issues/11710
                .WithVolume(
                    VolumeNameGenerator.Generate(builder, "data"),
                    "/var/lib/postgresql/18/docker"
                )
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
    public static IResourceBuilder<AzureManagedRedisResource> RunAsLocalContainer(
        this IResourceBuilder<AzureManagedRedisResource> builder
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

    /// <summary>
    ///     Adds Azure Storage Explorer to the parent Azure Storage resource of the specified blob container.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Blob Storage Container.</param>
    /// <returns>The updated resource builder with Azure Storage Explorer configured on the parent resource.</returns>
    /// <remarks>
    ///     This method retrieves the parent Azure Storage resource and applies the Azure Storage Explorer
    ///     configuration to it, enabling visual inspection and management of storage resources during development.
    /// </remarks>
    public static IResourceBuilder<AzureBlobStorageContainerResource> WithAzureStorageExplorer(
        this IResourceBuilder<AzureBlobStorageContainerResource> builder
    )
    {
        var storageContainerResource = builder.ApplicationBuilder.CreateResourceBuilder(
            builder.Resource.Parent
        );

        storageContainerResource.WithAzureStorageExplorer();

        return builder;
    }
}
