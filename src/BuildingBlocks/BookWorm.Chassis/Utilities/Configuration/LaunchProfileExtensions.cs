using BookWorm.Constants.Aspire;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Utilities.Configuration;

public static class LaunchProfileExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public bool IsHttpsLaunchProfile()
        {
            return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Protocols.Https;
        }

        public string GetScheme()
        {
            return builder.IsHttpsLaunchProfile() ? Protocols.Https : Protocols.Http;
        }
    }
}
