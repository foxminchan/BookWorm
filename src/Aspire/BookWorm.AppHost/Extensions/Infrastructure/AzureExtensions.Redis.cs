using Azure.Provisioning.Redis;
using Microsoft.Extensions.Hosting;
using RedisResource = Azure.Provisioning.Redis.RedisResource;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureManagedRedisResource> builder)
    {
        public IResourceBuilder<AzureManagedRedisResource> RunAsLocalContainer()
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

        public IResourceBuilder<AzureManagedRedisResource> ProvisionAsService(
            IHostEnvironment environment
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
                    Family = environment.IsProduction()
                        ? RedisSkuFamily.Premium
                        : RedisSkuFamily.BasicOrStandard,
                    Name = (environment.IsDevelopment(), environment.IsProduction()) switch
                    {
                        (true, _) => RedisSkuName.Basic,
                        (false, true) => RedisSkuName.Premium,
                        _ => RedisSkuName.Standard,
                    },
                    Capacity = (environment.IsDevelopment(), environment.IsStaging()) switch
                    {
                        (true, _) => 0,
                        (false, true) => 2,
                        _ => 1,
                    },
                };
            });

            return builder;
        }
    }
}
