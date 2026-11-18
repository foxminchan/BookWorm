using Azure.Provisioning.RedisEnterprise;
using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureManagedRedisResource> builder)
    {
        public IResourceBuilder<AzureManagedRedisResource> RunAsLocalContainer()
        {
            builder.RunAsContainer(config =>
                config
                    .WithDataVolume()
                    .WithRedisInsight()
                    .WithLifetime(ContainerLifetime.Persistent)
            );

            return builder;
        }

        public IResourceBuilder<AzureManagedRedisResource> ProvisionAsService(
            IHostEnvironment environment
        )
        {
            builder.ConfigureInfrastructure(infra =>
            {
                var cluster = infra
                    .GetProvisionableResources()
                    .OfType<RedisEnterpriseCluster>()
                    .FirstOrDefault();

                if (cluster is null)
                {
                    return;
                }

                cluster.Sku = new()
                {
                    Name = (environment.IsDevelopment(), environment.IsStaging()) switch
                    {
                        (true, _) => RedisEnterpriseSkuName.BalancedB0,
                        (false, true) => RedisEnterpriseSkuName.BalancedB1,
                        _ => RedisEnterpriseSkuName.BalancedB5,
                    },
                };

                cluster.HighAvailability = environment.IsDevelopment()
                    ? RedisEnterpriseHighAvailability.Disabled
                    : RedisEnterpriseHighAvailability.Enabled;
            });

            return builder;
        }
    }
}
