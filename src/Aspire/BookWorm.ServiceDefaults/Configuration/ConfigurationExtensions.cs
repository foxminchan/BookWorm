namespace BookWorm.ServiceDefaults.Configuration;

public static class ConfigurationExtensions
{
    public static void Configure<TSetting>(
        this IServiceCollection services,
        string section,
        string? name = null,
        Action<TSetting>? configure = null
    )
        where TSetting : class, new()
    {
        services
            .AddOptionsWithValidateOnStart<TSetting>(name)
            .Configure(options => configure?.Invoke(options))
            .BindConfiguration(section)
            .ValidateDataAnnotations();

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TSetting>>();
            var setting = options.Value;
            return setting;
        });
    }

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
