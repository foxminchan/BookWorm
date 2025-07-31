namespace BookWorm.ServiceDefaults.Configuration;

public static class LaunchProfileExtensions
{
    public static bool IsHttpsLaunchProfile(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Protocols.Https;
    }

    public static string GetScheme(this IHostApplicationBuilder builder)
    {
        return builder.IsHttpsLaunchProfile() ? Protocols.Https : Protocols.Http;
    }
}
