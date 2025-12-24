using Aspire.Hosting.Azure.AppContainers;
using Azure.Core;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.PostgreSql;
using Azure.Provisioning.Redis;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static partial class AzureExtensions
{
    /// <summary>
    ///     Provisions an Azure Storage resource with BookWorm-specific configuration.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Storage.</param>
    /// <returns>The configured resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method configures the Azure Storage resource with:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Standard LRS (Locally Redundant Storage) SKU</description>
    ///         </item>
    ///         <item>
    ///             <description>Southeast Asia location</description>
    ///         </item>
    ///         <item>
    ///             <description>Cool access tier for cost optimization</description>
    ///         </item>
    ///         <item>
    ///             <description>Environment and project tags for resource management</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static IResourceBuilder<AzureStorageResource> ProvisionAsService(
        this IResourceBuilder<AzureStorageResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var resource = infra
                .GetProvisionableResources()
                .OfType<StorageAccount>()
                .FirstOrDefault();

            if (resource is null)
            {
                return;
            }

            resource.Sku = new() { Name = StorageSkuName.StandardLrs };

            resource.Location = AzureLocation.SoutheastAsia;

            resource.AccessTier = StorageAccountAccessTier.Cool;
        });

        return builder;
    }

    /// <summary>
    ///     Provisions an Azure PostgreSQL Flexible Server resource with BookWorm-specific configuration.
    /// </summary>
    /// <param name="builder">The resource builder for Azure PostgreSQL Flexible Server.</param>
    /// <returns>The configured resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method configures the PostgreSQL Flexible Server with:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Burstable tier SKU for cost-effective workloads</description>
    ///         </item>
    ///         <item>
    ///             <description>Southeast Asia location</description>
    ///         </item>
    ///         <item>
    ///             <description>Zone-redundant high availability with standby in zone 2</description>
    ///         </item>
    ///         <item>
    ///             <description>7-day backup retention with geo-redundant backup disabled</description>
    ///         </item>
    ///         <item>
    ///             <description>32 GB storage with auto-grow disabled</description>
    ///         </item>
    ///         <item>
    ///             <description>Environment and project tags for resource management</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static IResourceBuilder<AzurePostgresFlexibleServerResource> ProvisionAsService(
        this IResourceBuilder<AzurePostgresFlexibleServerResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var resource = infra
                .GetProvisionableResources()
                .OfType<PostgreSqlFlexibleServer>()
                .FirstOrDefault();

            if (resource is null)
            {
                return;
            }

            resource.Sku = new() { Tier = PostgreSqlFlexibleServerSkuTier.Burstable };

            resource.Location = AzureLocation.SoutheastAsia;

            resource.HighAvailability = new()
            {
                Mode = PostgreSqlFlexibleServerHighAvailabilityMode.ZoneRedundant,
                StandbyAvailabilityZone = "2",
            };

            resource.Backup = new()
            {
                BackupRetentionDays = 7,
                GeoRedundantBackup = PostgreSqlFlexibleServerGeoRedundantBackupEnum.Disabled,
            };

            resource.Storage = new() { StorageSizeInGB = 32, AutoGrow = StorageAutoGrow.Disabled };
        });

        return builder;
    }

    /// <summary>
    ///     Provisions an Azure Redis Cache resource with BookWorm-specific configuration.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Redis Cache.</param>
    /// <returns>The configured resource builder for method chaining.</returns>
    /// <remarks>
    ///     This method configures the Redis Cache with:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Basic SKU with capacity 1 for development/testing workloads</description>
    ///         </item>
    ///         <item>
    ///             <description>Southeast Asia location</description>
    ///         </item>
    ///         <item>
    ///             <description>Environment and project tags for resource management</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static IResourceBuilder<AzureManagedRedisResource> ProvisionAsService(
        this IResourceBuilder<AzureManagedRedisResource> builder
    )
    {
        builder.ConfigureInfrastructure(infra =>
        {
            var resource = infra
                .GetProvisionableResources()
                .OfType<RedisResource>()
                .FirstOrDefault();

            if (resource is null)
            {
                return;
            }

            resource.Sku = new()
            {
                Family = RedisSkuFamily.BasicOrStandard,
                Name = RedisSkuName.Basic,
                Capacity = 1,
            };

            resource.Location = AzureLocation.SoutheastAsia;
        });

        return builder;
    }

    /// <summary>
    ///     Provisions an Azure Container App Environment resource with BookWorm-specific configuration.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Container App Environment.</param>
    /// <remarks>
    ///     This method configures the Container App Environment with:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Dashboard integration for monitoring</description>
    ///         </item>
    ///         <item>
    ///             <description>Azure Developer CLI (azd) resource naming conventions</description>
    ///         </item>
    ///         <item>
    ///             <description>Consumption workload profile for serverless execution</description>
    ///         </item>
    ///         <item>
    ///             <description>Southeast Asia location</description>
    ///         </item>
    ///         <item>
    ///             <description>Environment and project tags for resource management</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static void ProvisionAsService(
        this IResourceBuilder<AzureContainerAppEnvironmentResource> builder
    )
    {
        builder
            .WithDashboard()
            .WithAzdResourceNaming()
            .ConfigureInfrastructure(infra =>
            {
                var resource = infra
                    .GetProvisionableResources()
                    .OfType<ContainerAppManagedEnvironment>()
                    .FirstOrDefault();

                if (resource is null)
                {
                    return;
                }

                resource.WorkloadProfiles.Add(
                    new ContainerAppWorkloadProfile
                    {
                        Name = "Consumption",
                        WorkloadProfileType = "Consumption",
                    }
                );

                resource.Location = AzureLocation.SoutheastAsia;
            });
    }
}
