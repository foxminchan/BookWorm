using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookWorm.Chassis.Utilities.Configurations;

public static class ConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void Configure<TSetting>(
            string section,
            string? name = null,
            Action<TSetting>? configure = null
        )
            where TSetting : class
        {
            var services = builder.Services;

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

        public IServiceCollection AddAppSettings<T>()
            where T : AppSettings, new()
        {
            var services = builder.Services;

            services.AddSingleton<T>(_ =>
            {
                var settings = AppSettings.Parse<T>(builder.Configuration);
                return settings;
            });

            return services;
        }
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
