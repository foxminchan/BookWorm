namespace BookWorm.AppHost.Extensions;

public static class LaunchProfileExtensions
{
    /// <summary>
    ///     Determines whether the application is running with an HTTPS launch profile.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IDistributedApplicationBuilder" /> instance used to access the application configuration.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the "DOTNET_LAUNCH_PROFILE" configuration value is set to HTTPS; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsHttpsLaunchProfile(this IDistributedApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Protocol.Https;
    }
}
