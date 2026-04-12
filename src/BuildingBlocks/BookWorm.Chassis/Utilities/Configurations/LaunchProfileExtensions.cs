using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Utilities.Configurations;

public static class LaunchProfileExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Determines whether the current launch profile uses the HTTPS scheme.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> when `DOTNET_LAUNCH_PROFILE` equals <see cref="Uri.UriSchemeHttps" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool IsHttpsLaunchProfile()
        {
            return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Uri.UriSchemeHttps;
        }

        /// <summary>
        ///     Gets the URL scheme for the current launch profile.
        /// </summary>
        /// <returns><see cref="Uri.UriSchemeHttps" /> for HTTPS launch profiles; otherwise, <see cref="Uri.UriSchemeHttp" />.</returns>
        public string GetScheme()
        {
            return builder.IsHttpsLaunchProfile() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        }
    }
}
