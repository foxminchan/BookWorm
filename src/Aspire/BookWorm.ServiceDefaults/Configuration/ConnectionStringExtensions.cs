using Microsoft.Extensions.Configuration;

namespace BookWorm.ServiceDefaults.Configuration;

public static class ConnectionStringExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Configuration missing value for: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}"
            );
        }

        return connectionString;
    }
}
