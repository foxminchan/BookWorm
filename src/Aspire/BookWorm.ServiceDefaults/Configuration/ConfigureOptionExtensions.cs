using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BookWorm.ServiceDefaults.Configuration;

public static class ConfigureOptionExtensions
{
    public static void Configure<TSetting>(
        this IServiceCollection services,
        string section,
        string? name = null
    )
        where TSetting : class, new()
    {
        services
            .AddOptionsWithValidateOnStart<TSetting>(name)
            .BindConfiguration(section)
            .ValidateDataAnnotations();

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TSetting>>();
            var setting = options.Value;
            return setting;
        });
    }
}
