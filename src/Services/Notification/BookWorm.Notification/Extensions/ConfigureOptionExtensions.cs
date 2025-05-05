namespace BookWorm.Notification.Extensions;

public static class ConfigureOptionExtensions
{
    public static void Configure<TSetting>(
        this IServiceCollection services,
        string section,
        string? name = null
    )
        where TSetting : class, new()
    {
        var setting = new TSetting();

        services
            .AddOptionsWithValidateOnStart<TSetting>(name)
            .BindConfiguration(section)
            .ValidateDataAnnotations();

        services.AddSingleton(setting);
    }
}
