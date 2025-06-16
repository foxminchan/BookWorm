using Aspire.Hosting.Azure.AppContainers;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.PostgreSql;
using Azure.Provisioning.Redis;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost.Extensions.Infrastructure;

/// <summary>
///     Provides extension methods for configuring Azure resources for production deployment provisioning.
///     This partial class handles infrastructure configuration, SKU settings, capacity planning,
///     and tagging strategies for Azure services when deployed to the cloud.
/// </summary>
/// <remarks>
///     <para>
///         This class focuses on production-ready configurations for Azure resources including
///         - Storage accounts with Premium ZRS and Hot access tier
///         - SignalR services with Premium_P1 SKU and 10 capacity units
///         - PostgreSQL Flexible Servers with Standard_B1ms burstable tier
///         - Redis Cache with Basic SKU and 1 capacity unit
///         - Container App Environments with standardized naming and tagging
///     </para>
///     <para>
///         All provisioned resources are automatically tagged with environment and project information
///         for resource management and cost tracking purposes.
///     </para>
///     <para>
///         These methods only affect resource provisioning during Azure deployment (publish mode)
///         and have no impact on local development execution.
///     </para>
/// </remarks>
public static partial class AzureExtensions
{
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
            var resource = infra.GetProvisionableResources().OfType<StorageAccount>().Single();

            resource.Sku = new() { Name = StorageSkuName.StandardLrs };

            resource.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            resource.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

        return builder;
    }

    /// <summary>
    ///     Configures the Azure SignalR resource to be provisioned as a service with specific infrastructure settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure SignalR.</param>
    /// <returns>The updated resource builder.</returns>
    public static IResourceBuilder<AzureSignalRResource> ProvisionAsService(
        this IResourceBuilder<AzureSignalRResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var resource = infra.GetProvisionableResources().OfType<SignalRService>().Single();

            resource.Sku.Name = "Free_F1";
            resource.PublicNetworkAccess = "Enabled";

            resource.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            resource.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

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
            var resource = infra
                .GetProvisionableResources()
                .OfType<PostgreSqlFlexibleServer>()
                .Single();

            resource.Sku = new()
            {
                Name = "Standard_B1ms",
                Tier = PostgreSqlFlexibleServerSkuTier.Burstable,
            };

            resource.Tags.Add(
                nameof(Environment),
                builder.ApplicationBuilder.Environment.EnvironmentName
            );
            resource.Tags.Add(nameof(Projects), nameof(BookWorm));
        });

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

    /// <summary>
    ///     Configures the Azure Container App Environment resource to be provisioned as a service with specific infrastructure
    ///     settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Container App Environment.</param>
    /// <returns>The updated resource builder.</returns>
    public static void ProvisionAsService(
        this IResourceBuilder<AzureContainerAppEnvironmentResource> builder
    )
    {
        builder
            .WithAzdResourceNaming()
            .ConfigureInfrastructure(infra =>
            {
                var resource = infra
                    .GetProvisionableResources()
                    .OfType<ContainerAppManagedEnvironment>()
                    .Single();

                resource.Tags.Add(
                    nameof(Environment),
                    builder.ApplicationBuilder.Environment.EnvironmentName
                );
                resource.Tags.Add(nameof(Projects), nameof(BookWorm));
            });
    }
}
