using Azure.Provisioning.PostgreSql;
using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzurePostgresFlexibleServerResource> builder)
    {
        public IResourceBuilder<AzurePostgresFlexibleServerResource> RunAsLocalContainer()
        {
            builder.RunAsContainer(cfg =>
                cfg.WithPgAdmin()
                    .WithDataVolume()
                    .WithImagePullPolicy(ImagePullPolicy.Always)
                    .WithLifetime(ContainerLifetime.Persistent)
            );

            return builder;
        }

        public IResourceBuilder<AzurePostgresFlexibleServerResource> ProvisionAsService(
            IHostEnvironment environment
        )
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

                var isDevelopment = environment.IsDevelopment();
                var isStaging = environment.IsStaging();

                resource.Sku = new()
                {
                    Name = (isDevelopment, isStaging) switch
                    {
                        (true, _) => "Standard_B1ms",
                        (false, true) => "Standard_D2ds_v5",
                        _ => "Standard_D4ds_v5",
                    },
                    Tier = isDevelopment
                        ? PostgreSqlFlexibleServerSkuTier.Burstable
                        : PostgreSqlFlexibleServerSkuTier.GeneralPurpose,
                };

                resource.HighAvailability = new()
                {
                    Mode = isDevelopment
                        ? PostgreSqlFlexibleServerHighAvailabilityMode.Disabled
                        : PostgreSqlFlexibleServerHighAvailabilityMode.ZoneRedundant,
                };

                if (!isDevelopment)
                {
                    resource.HighAvailability.StandbyAvailabilityZone = "2";
                }

                resource.Backup = new()
                {
                    BackupRetentionDays = (isDevelopment, isStaging) switch
                    {
                        (true, _) => 7,
                        (false, true) => 14,
                        _ => 35,
                    },
                    GeoRedundantBackup = environment.IsProduction()
                        ? PostgreSqlFlexibleServerGeoRedundantBackupEnum.Enabled
                        : PostgreSqlFlexibleServerGeoRedundantBackupEnum.Disabled,
                };

                resource.Storage = new()
                {
                    StorageSizeInGB = (isDevelopment, isStaging) switch
                    {
                        (true, _) => 32,
                        (false, true) => 128,
                        _ => 256,
                    },
                    AutoGrow = isDevelopment ? StorageAutoGrow.Disabled : StorageAutoGrow.Enabled,
                };

                if (!isDevelopment)
                {
                    infra.Add(
                        new PostgreSqlFlexibleServerConfiguration("pgbouncer")
                        {
                            Parent = resource,
                            Name = "pgbouncer",
                            Value = "on",
                        }
                    );
                }
            });

            return builder;
        }
    }
}
