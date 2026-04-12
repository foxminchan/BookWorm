using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookWorm.Chassis.Utilities.Configurations;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        ///     Retrieves a named connection string from configuration and throws if the value is missing or empty.
        /// </summary>
        /// <param name="name">The connection string key under the <c>ConnectionStrings</c> section.</param>
        /// <returns>The configured connection string value.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the connection string is not found or is empty.
        /// </exception>
        public string GetRequiredConnectionString(string name)
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

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers, binds, and validates a settings type from a configuration section.
        /// </summary>
        /// <typeparam name="TSetting">The settings type to bind.</typeparam>
        /// <param name="section">The configuration section path.</param>
        /// <param name="name">An optional named options instance.</param>
        /// <param name="configure">An optional callback to apply additional in-memory configuration.</param>
        /// <remarks>
        ///     This method enables startup validation and data annotation validation, then exposes
        ///     the resolved settings instance as a singleton for direct dependency injection.
        /// </remarks>
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

            // Expose the bound options value directly for consumers that depend on TSetting.
            services.TryAddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TSetting>>();
                var setting = options.Value;
                return setting;
            });
        }

        /// <summary>
        ///     Parses and registers application settings as a singleton instance.
        /// </summary>
        /// <typeparam name="T">The concrete app settings type.</typeparam>
        /// <returns>The updated service collection.</returns>
        /// <remarks>
        ///     This helper uses <see cref="AppSettings.Parse{T}(IConfiguration)" /> to materialize
        ///     configuration once at startup and register it for DI consumers.
        /// </remarks>
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
}
