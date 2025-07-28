using BookWorm.Constants.Aspire;

namespace BookWorm.Finance.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        builder.AddAzureNpgsqlDbContext<FinanceDbContext>(
            Components.Database.Finance,
            configureDbContextOptions: options => options.UseSnakeCaseNamingConvention()
        );

        services.AddMigration<FinanceDbContext>();

        builder.AddSagaStateMachineServices();
    }
}
