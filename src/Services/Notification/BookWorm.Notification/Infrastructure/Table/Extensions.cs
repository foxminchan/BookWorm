namespace BookWorm.Notification.Infrastructure.Table;

internal static class Extensions
{
    public static void AddTableService(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAzureTableClient(Components.Azure.Storage.Table);

        services.AddScoped<ITableService, TableService>();
    }
}
