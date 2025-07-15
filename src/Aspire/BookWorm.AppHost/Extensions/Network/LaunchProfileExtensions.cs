namespace BookWorm.AppHost.Extensions.Network;

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
    /// <remarks>
    ///     This method checks the <c>DOTNET_LAUNCH_PROFILE</c> environment variable to determine the security protocol:
    ///     - Compares the configuration value against <c>Protocol.Https</c> for exact match
    ///     - Used to conditionally apply HTTPS-specific configurations and security enhancements
    ///     - Enables different behavior for secure vs. insecure launch profiles
    ///     - Commonly used for endpoint visibility, security policies, and development experience optimization
    /// </remarks>
    /// <example>
    ///     <code>
    ///     if (builder.IsHttpsLaunchProfile())
    ///     {
    ///         builder.HidePlainHttpLink();
    ///     }
    ///     </code>
    /// </example>
    public static bool IsHttpsLaunchProfile(this IDistributedApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Protocol.Https;
    }

    /// <summary>
    ///     Gets the current launch profile name based on the application's security configuration.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IDistributedApplicationBuilder" /> instance used to access the application configuration.
    /// </param>
    /// <returns>
    ///     Returns <c>Protocol.Https</c> if running with HTTPS launch profile; otherwise, returns <c>Protocol.Http</c>.
    /// </returns>
    /// <remarks>
    ///     This method provides a convenient way to retrieve the appropriate protocol name:
    ///     - Uses <see cref="IsHttpsLaunchProfile" /> internally to determine the current launch profile
    ///     - Returns standardized protocol constants from the <c>Protocol</c> class
    ///     - Useful for endpoint configuration, URL generation, and display text formatting
    ///     - Ensures consistency in protocol naming across the application
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var protocolName = builder.GetLaunchProfileName();
    ///     var displayText = $"API ({protocolName.ToUpperInvariant()})";
    ///     </code>
    /// </example>
    public static string GetLaunchProfileName(this IDistributedApplicationBuilder builder)
    {
        return builder.IsHttpsLaunchProfile() ? Protocol.Https : Protocol.Http;
    }
}
