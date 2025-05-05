namespace BookWorm.Notification.Extensions;

public static class ConnectionStringExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{name}' not found.");
        }

        return connectionString;
    }
}
