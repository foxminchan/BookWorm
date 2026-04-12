using BookWorm.Constants.Aspire;

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
        }
    }
}
