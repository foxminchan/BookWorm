using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.Constants.Aspire;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace BookWorm.Finance.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddAzurePostgresDbContext<FinanceDbContext>(
                Components.Database.Finance,
                _ => services.AddMigration<FinanceDbContext>(),
                true
            );

            builder.AddSagaStateMachineServices();

            var postgresCs = builder.Configuration.GetConnectionString(Components.Database.Finance);
            builder.AddEventBus(opts =>
            {
                if (!string.IsNullOrWhiteSpace(postgresCs))
                {
                    opts.PersistMessagesWithPostgresql(postgresCs, "wolverine");
                    opts.UseEntityFrameworkCoreTransactions();
                }

                // Preserve per-order saga ordering on Kafka by routing all events that
                // expose an OrderId to the same partition.
                opts.MessagePartitioning.ByPropertyNamed("OrderId");

                opts.Discovery.IncludeAssembly(typeof(IFinanceApiMarker).Assembly);
                opts.ListenToIntegrationEventsIn(typeof(IFinanceApiMarker).Assembly);
            });
        }
    }
}
