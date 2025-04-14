using Aspire.Hosting.Azure;
using Azure.Provisioning.PostgreSql;
using Azure.Provisioning.Redis;
using Azure.Provisioning.SignalR;
using Azure.Provisioning.Storage;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost.Extensions;

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
            var signalRService = infra
                .GetProvisionableResources()
                .OfType<SignalRService>()
                .Single();

            signalRService.Sku.Name = "Premium_P1";
            signalRService.Sku.Capacity = 10;
            signalRService.PublicNetworkAccess = "Enabled";
            signalRService.Tags.Add(nameof(Projects), nameof(BookWorm));
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
