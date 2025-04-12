using Aspire.Hosting.Azure;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.PostgreSql;
using Azure.Provisioning.Redis;
using Azure.Provisioning.Storage;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost.Extensions;

public static class AzureExtensions
{
    /// <summary>
    ///     Configures the Azure Cosmos DB resource to run as a container or use access key authentication based on the
    ///     execution context.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Cosmos DB.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureCosmosDBResource> RunAsContainer(
        this IResourceBuilder<AzureCosmosDBResource> builder
    )
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            builder.RunAsPreviewEmulator(config =>
                config
                    .WithDataExplorer()
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

    /// <summary>
    ///     Configures the Azure Cosmos DB resource to be provisioned as a service with specific infrastructure settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Cosmos DB.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureCosmosDBResource> ProvisionAsService(
        this IResourceBuilder<AzureCosmosDBResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var cosmosDbAccount = infra
                .GetProvisionableResources()
                .OfType<CosmosDBAccount>()
                .Single();

            cosmosDbAccount.Kind = CosmosDBAccountKind.GlobalDocumentDB;
            cosmosDbAccount.ConsistencyPolicy = new()
            {
                DefaultConsistencyLevel = DefaultConsistencyLevel.Session,
            };
            cosmosDbAccount.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            cosmosDbAccount.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

        return builder;
    }

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
    ///     Configures the Azure Storage resource to be provisioned as a service with specific infrastructure settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Storage.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureStorageResource> ProvisionAsService(
        this IResourceBuilder<AzureStorageResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var storageAccount = infra
                .GetProvisionableResources()
                .OfType<StorageAccount>()
                .Single();

            storageAccount.AccessTier = StorageAccountAccessTier.Hot;
            storageAccount.Sku = new() { Name = StorageSkuName.PremiumZrs };
            storageAccount.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            storageAccount.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

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
                cfg.WithPgAdmin()
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
    ///     Configures the Azure PostgreSQL Flexible Server resource to be provisioned as a service with specific
    ///     infrastructure settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure PostgreSQL Flexible Server.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzurePostgresFlexibleServerResource> ProvisionAsService(
        this IResourceBuilder<AzurePostgresFlexibleServerResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var flexibleServer = infra
                .GetProvisionableResources()
                .OfType<PostgreSqlFlexibleServer>()
                .Single();

            flexibleServer.Sku = new() { Tier = PostgreSqlFlexibleServerSkuTier.GeneralPurpose };
            flexibleServer.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            flexibleServer.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

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

    /// <summary>
    ///     Configures the Azure Redis Cache resource to be provisioned as a service with specific infrastructure settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Redis Cache.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureRedisCacheResource> ProvisionAsService(
        this IResourceBuilder<AzureRedisCacheResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var resource = infra.GetProvisionableResources().OfType<RedisResource>().Single();

            resource.Sku = new()
            {
                Family = RedisSkuFamily.BasicOrStandard,
                Name = RedisSkuName.Basic,
                Capacity = 1,
            };
            resource.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            resource.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

        return builder;
    }
}
