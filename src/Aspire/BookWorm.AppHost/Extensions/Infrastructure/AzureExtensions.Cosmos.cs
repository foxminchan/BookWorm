using Azure.Provisioning.CosmosDB;
using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureCosmosDBResource> builder)
    {
        public IResourceBuilder<AzureCosmosDBResource> RunAsLocalContainer()
        {
            builder.RunAsEmulator(cfg =>
                cfg.WithDataExplorer().WithDataVolume().WithLifetime(ContainerLifetime.Persistent)
            );

            return builder;
        }

        public IResourceBuilder<AzureCosmosDBResource> ProvisionAsService(
            IHostEnvironment environment
        )
        {
            builder.ConfigureInfrastructure(infra =>
            {
                var resource = infra
                    .GetProvisionableResources()
                    .OfType<CosmosDBAccount>()
                    .FirstOrDefault();

                if (resource is null)
                {
                    return;
                }

                if (!environment.IsProduction())
                {
                    resource.Capabilities.Add(
                        new CosmosDBAccountCapability { Name = "EnableServerless" }
                    );
                }

                if (!environment.IsDevelopment())
                {
                    resource.DisableLocalAuth = true;
                }
            });

            return builder;
        }
    }
}
