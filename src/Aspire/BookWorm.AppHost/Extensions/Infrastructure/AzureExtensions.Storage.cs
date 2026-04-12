using Azure.Core;

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

        public IResourceBuilder<AzureStorageResource> ProvisionAsService(
            params IResourceBuilder<ParameterResource>[] origins
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
                };

                if (origins.Length > 0)
                {
                    foreach (var origin in origins)
                    {
                        corsRule.AllowedOrigins.Add(origin.AsProvisioningParameter(infra));
                    }
                }
                else
                {
                    corsRule.AllowedOrigins = ["*"];
                }

                var blobService = new BlobService(nameof(BlobService).ToLowerInvariant())
                {
                    Parent = resource,
                    CorsRules = [corsRule],
                };

                infra.Add(blobService);
            });

            return builder;
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
