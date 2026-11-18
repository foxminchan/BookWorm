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
                    Name = (environment.IsDevelopment(), environment.IsStaging()) switch
                    {
                        (true, _) => StorageSkuName.StandardLrs,
                        (false, true) => StorageSkuName.StandardZrs,
                        _ => StorageSkuName.StandardGzrs,
                    },
                };

                resource.AccessTier = StorageAccountAccessTier.Hot;
                resource.AllowBlobPublicAccess = false;
                resource.MinimumTlsVersion = StorageMinimumTlsVersion.Tls12;

                var corsRule = new StorageCorsRule
                {
                    AllowedMethods =
                    [
                        CorsRuleAllowedMethod.Get,
                        CorsRuleAllowedMethod.Head,
                        CorsRuleAllowedMethod.Options,
                    ],
                    AllowedHeaders = ["*"],
                    ExposedHeaders = ["*"],
                    MaxAgeInSeconds = 3600,
                    AllowedOrigins = ["*"],
                };

                var existingBlobService = infra
                    .GetProvisionableResources()
                    .OfType<BlobService>()
                    .FirstOrDefault();

                if (existingBlobService is not null)
                {
                    existingBlobService.CorsRules = [corsRule];
                }
                else
                {
                    var blobService = new BlobService(nameof(BlobService).ToLowerInvariant())
                    {
                        Parent = resource,
                        CorsRules = [corsRule],
                    };

                    infra.Add(blobService);
                }
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
