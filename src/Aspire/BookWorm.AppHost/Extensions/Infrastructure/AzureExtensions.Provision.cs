using Aspire.Hosting.Azure.AppContainers;
using Azure.Provisioning;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.PostgreSql;
using Azure.Provisioning.Redis;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static partial class AzureExtensions
{
    /// <summary>
    ///     Configures the Azure Storage resource to be provisioned as a service with specific infrastructure settings.
    /// </summary>
    /// <param name="builder">The resource builder for Azure Storage.</param>
    /// <returns>The updated resource builder with configured infrastructure settings.</returns>
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
    /// <returns>The updated resource builder with configured infrastructure settings.</returns>
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
    /// <returns>The updated resource builder with configured infrastructure settings.</returns>
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

            infra.Add(
                new ProvisioningOutput("hostname", typeof(string))
                {
                    Value = resource.FullyQualifiedDomainName,
                }
            );

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
    /// <returns>The updated resource builder with configured infrastructure settings.</returns>
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
