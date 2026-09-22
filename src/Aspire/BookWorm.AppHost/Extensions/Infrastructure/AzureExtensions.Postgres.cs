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

                IResourceBuilder<AzurePostgresFlexibleServerResource>.ConfigureResource(
                    resource,
                    environment
                );

                if (!environment.IsDevelopment())
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

        private static void ConfigureResource(
            PostgreSqlFlexibleServer resource,
            IHostEnvironment environment
        )
        {
            var isDevelopment = environment.IsDevelopment();
            var isStaging = environment.IsStaging();

            resource.Sku = IResourceBuilder<AzurePostgresFlexibleServerResource>.CreateSku(
                isDevelopment,
                isStaging
            );
            resource.HighAvailability =
                IResourceBuilder<AzurePostgresFlexibleServerResource>.CreateHighAvailability(
                    isDevelopment
                );

            if (!isDevelopment)
            {
                resource.HighAvailability.StandbyAvailabilityZone = "2";
            }

            resource.Backup = IResourceBuilder<AzurePostgresFlexibleServerResource>.CreateBackup(
                isDevelopment,
                isStaging,
                environment
            );
            resource.Storage = IResourceBuilder<AzurePostgresFlexibleServerResource>.CreateStorage(
                isDevelopment,
                isStaging
            );
        }

        private static PostgreSqlFlexibleServerSku CreateSku(bool isDevelopment, bool isStaging)
        {
            return new()
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
        }

        private static PostgreSqlFlexibleServerHighAvailability CreateHighAvailability(
            bool isDevelopment
        )
        {
            return new()
            {
                Mode = isDevelopment
                    ? PostgreSqlFlexibleServerHighAvailabilityMode.Disabled
                    : PostgreSqlFlexibleServerHighAvailabilityMode.ZoneRedundant,
            };
        }

        private static PostgreSqlFlexibleServerBackupProperties CreateBackup(
            bool isDevelopment,
            bool isStaging,
            IHostEnvironment environment
        )
        {
            return new()
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
        }

        private static PostgreSqlFlexibleServerStorage CreateStorage(
            bool isDevelopment,
            bool isStaging
        )
        {
            return new()
            {
                StorageSizeInGB = (isDevelopment, isStaging) switch
                {
                    (true, _) => 32,
                    (false, true) => 128,
                    _ => 256,
                },
                AutoGrow = isDevelopment ? StorageAutoGrow.Disabled : StorageAutoGrow.Enabled,
            };
        }
    }
}
