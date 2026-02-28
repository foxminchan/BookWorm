using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

namespace BookWorm.Chassis.Utilities.Configurations;

public abstract class AppSettings
{
    public virtual OpenApiInfo? OpenApi { get; set; } = new();

    public static T Parse<T>(IConfiguration config)
        where T : AppSettings, new()
    {
        var settings = new T();
        config.Bind(settings);
        return settings;
    }
}
