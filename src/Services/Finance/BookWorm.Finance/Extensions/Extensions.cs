using BookWorm.Constants.Aspire;

namespace BookWorm.Finance.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
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
