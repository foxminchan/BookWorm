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
    /// <remarks>
    ///     This method configures the Azure Storage Account with:
    ///     - Standard LRS (Locally Redundant Storage) SKU for cost efficiency
    ///     - Environment and project tags for resource management
    ///     - Production-ready settings for cloud deployment
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureStorage("storage")
    ///            .ProvisionAsService();
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method configures the Azure SignalR Service with:
    ///     - Free F1 SKU for development and testing
    ///     - Public network access enabled for client connections
    ///     - Environment and project tags for resource management
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureSignalR("signalr")
    ///            .ProvisionAsService();
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method configures the Azure PostgreSQL Flexible Server with:
    ///     - Standard_B1ms SKU with Burstable tier for cost-effective performance
    ///     - Environment and project tags for resource management
    ///     - Production-ready database settings for cloud deployment
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzurePostgresFlexibleServer("postgres")
    ///            .ProvisionAsService();
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method configures the Azure Redis Cache with:
    ///     - Basic SKU with 1GB capacity for cost-effective caching
    ///     - BasicOrStandard family for development and production workloads
    ///     - Environment and project tags for resource management
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureRedis("redis")
    ///            .ProvisionAsService();
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method configures the Azure Container App Environment with:
    ///     - Azure Developer CLI (azd) resource naming conventions
    ///     - Environment and project tags for resource management
    ///     - Production-ready container hosting environment
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddAzureContainerAppEnvironment("env")
    ///            .ProvisionAsService();
    ///     </code>
    /// </example>
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

    /// <summary>
    ///     Configures Azure Application Insights for the project resource during Azure deployment.
    ///     Adds a reference to the Application Insights resource if the execution context is in publish mode.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource.</param>
    /// <returns>The updated resource builder with Application Insights configured.</returns>
    /// <remarks>
    ///     This method conditionally configures Application Insights integration:
    ///     - Only applies when in publish mode (cloud deployment)
    ///     - Automatically locates existing Application Insights resource
    ///     - Creates reference for telemetry and monitoring integration
    ///     - Returns unchanged builder if not in publish mode
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddProject&lt;WebApi&gt;("api")
    ///            .WithAzApplicationInsights();
    ///     </code>
    /// </example>
    public static IResourceBuilder<ProjectResource> WithAzApplicationInsights(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        var applicationBuilder = builder.ApplicationBuilder;

        if (!applicationBuilder.ExecutionContext.IsPublishMode)
        {
            return builder;
        }

        var resource = applicationBuilder
            .Resources.OfType<AzureApplicationInsightsResource>()
            .Single();
        var applicationInsights = applicationBuilder.CreateResourceBuilder(resource);

        builder.WithReference(applicationInsights);

        return builder;
    }
}
