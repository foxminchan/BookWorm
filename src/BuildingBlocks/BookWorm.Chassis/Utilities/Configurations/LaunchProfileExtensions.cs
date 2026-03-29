using BookWorm.Constants.Core;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Utilities.Configurations;

public static class LaunchProfileExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public bool IsHttpsLaunchProfile()
        {
            return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Uri.UriSchemeHttps;
        }

        public string GetScheme()
        {
            return builder.IsHttpsLaunchProfile() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        }
    }
}
