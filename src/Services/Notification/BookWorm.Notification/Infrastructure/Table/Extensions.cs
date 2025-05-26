using BookWorm.Constants.Aspire;

namespace BookWorm.Notification.Infrastructure.Table;

public static class Extensions
{
    public static void AddTableService(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAzureTableClient(
            Components.Azure.Storage.Table,
            settings => settings.DisableHealthChecks = true
        );

        services.AddScoped<ITableService, TableService>();
    }
}
