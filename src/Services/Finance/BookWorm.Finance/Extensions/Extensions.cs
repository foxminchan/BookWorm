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

                opts.Discovery.IncludeAssembly(typeof(IFinanceApiMarker).Assembly);
            });
        }
    }
}
