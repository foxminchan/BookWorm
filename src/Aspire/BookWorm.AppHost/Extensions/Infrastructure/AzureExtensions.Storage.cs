using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureStorageResource> builder)
    {
        public IResourceBuilder<AzureStorageResource> RunAsLocalContainer()
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

        public void ProvisionAsService(IHostEnvironment environment)
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

                resource.Sku = new()
                {
                    Name = environment.IsDevelopment()
                        ? StorageSkuName.StandardLrs
                        : StorageSkuName.StandardGzrs,
                };

                resource.AccessTier = environment.IsDevelopment()
                    ? StorageAccountAccessTier.Cool
                    : StorageAccountAccessTier.Hot;

                var corsRule = new StorageCorsRule
                {
                    AllowedMethods =
                    [
                        CorsRuleAllowedMethod.Get,
                        CorsRuleAllowedMethod.Head,
                        CorsRuleAllowedMethod.Post,
                        CorsRuleAllowedMethod.Options,
                    ],
                    AllowedHeaders = ["*"],
                    ExposedHeaders = ["*"],
                    MaxAgeInSeconds = 3600,
                    AllowedOrigins = ["*"],
                };

                var blobService = new BlobService(nameof(BlobService).ToLowerInvariant())
                {
                    Parent = resource,
                    CorsRules = [corsRule],
                };

                infra.Add(blobService);
            });
        }
    }

    extension(IResourceBuilder<AzureBlobStorageContainerResource> builder)
    {
        public IResourceBuilder<AzureBlobStorageContainerResource> WithAzureStorageExplorer()
        {
            var storageContainerResource = builder.ApplicationBuilder.CreateResourceBuilder(
                builder.Resource.Parent
            );

            storageContainerResource.WithAzureStorageExplorer();

            return builder;
        }
    }
}
