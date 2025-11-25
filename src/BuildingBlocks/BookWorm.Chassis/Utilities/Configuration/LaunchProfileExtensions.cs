using BookWorm.Constants.Core;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Utilities.Configuration;

public static class LaunchProfileExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public bool IsHttpsLaunchProfile()
        {
            return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Http.Schemes.Https;
        }

        public string GetScheme()
        {
            return builder.IsHttpsLaunchProfile() ? Http.Schemes.Https : Http.Schemes.Http;
        }
    }
}
