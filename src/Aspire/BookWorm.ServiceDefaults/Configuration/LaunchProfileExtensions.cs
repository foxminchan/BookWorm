namespace BookWorm.ServiceDefaults.Configuration;

public static class LaunchProfileExtensions
{
    public static bool IsHttpsLaunchProfile(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Protocol.Https;
    }

    public static string GetScheme(this IHostApplicationBuilder builder)
    {
        return builder.IsHttpsLaunchProfile() ? Protocol.Https : Protocol.Http;
    }
}
