using Azure.Core;
using Azure.Provisioning.PostgreSql;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzurePostgresFlexibleServerResource> builder)
    {
        public IResourceBuilder<AzurePostgresFlexibleServerResource> RunAsLocalContainer()
        {
            builder.RunAsContainer(cfg =>
            {
                cfg.WithPgAdmin()
                    // Issue: https://github.com/dotnet/aspire/issues/11710
                    .WithVolume(
                        VolumeNameGenerator.Generate(builder, "data"),
                        "/var/lib/postgresql/18/docker"
                    )
                    .WithImageTag("18.3")
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent);
            });

            return builder;
        }

        public IResourceBuilder<AzurePostgresFlexibleServerResource> ProvisionAsService()
        {
            builder.ConfigureInfrastructure(infra =>
            {
                var resource = infra
                    .GetProvisionableResources()
                    .OfType<PostgreSqlFlexibleServer>()
                    .FirstOrDefault();

                if (resource is null)
                {
                    return;
                }

                resource.Sku = new() { Tier = PostgreSqlFlexibleServerSkuTier.Burstable };

                resource.Location = AzureLocation.SoutheastAsia;

                resource.HighAvailability = new()
                {
                    Mode = PostgreSqlFlexibleServerHighAvailabilityMode.ZoneRedundant,
                    StandbyAvailabilityZone = "2",
                };

                resource.Backup = new()
                {
                    BackupRetentionDays = 7,
                    GeoRedundantBackup = PostgreSqlFlexibleServerGeoRedundantBackupEnum.Disabled,
                };

                resource.Storage = new()
                {
                    StorageSizeInGB = 32,
                    AutoGrow = StorageAutoGrow.Disabled,
                };
            });

            return builder;
        }
    }
}
