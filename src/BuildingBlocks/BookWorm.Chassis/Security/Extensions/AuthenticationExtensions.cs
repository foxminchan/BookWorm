using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

namespace BookWorm.Chassis.Security.Extensions;

public static class AuthenticationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Configures the default JWT bearer authentication pipeline using Microsoft.Identity.Web
        ///     bound from the <c>AzureAd</c> configuration section injected by Aspire.
        /// </summary>
        /// <returns>The same <see cref="IHostApplicationBuilder" /> instance for fluent configuration.</returns>
        public IHostApplicationBuilder AddDefaultAuthentication()
        {
            builder
                .Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            return builder;
        }
    }
}
