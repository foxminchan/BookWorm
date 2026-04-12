using Azure.Core;
using Azure.Provisioning.Redis;
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

        public IResourceBuilder<AzureManagedRedisResource> ProvisionAsService()
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
    }
}
