using BookWorm.Chassis.EF;

namespace BookWorm.Chat.Infrastructure;

public static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add database configuration
        builder.AddAzureNpgsqlDbContext<ChatDbContext>(Components.Database.Chat);
        services.AddMigration<ChatDbContext>();
        services.AddRepositories(typeof(IChatApiMarker));
    }
}
