using BookWorm.Constants.Aspire;

namespace BookWorm.Finance.Infrastructure;

internal static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAzureNpgsqlDbContext<FinanceDbContext>(
            Components.Database.Finance,
            configureDbContextOptions: options => options.UseSnakeCaseNamingConvention()
        );

        services.AddMigration<FinanceDbContext>();
    }
}
