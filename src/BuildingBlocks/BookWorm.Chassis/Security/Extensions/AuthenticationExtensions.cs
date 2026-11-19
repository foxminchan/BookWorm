using BookWorm.Chassis.Security.TokenExchange;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

namespace BookWorm.Chassis.Security.Extensions;

public static class AuthenticationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Configures the default JWT bearer authentication pipeline using Microsoft.Identity.Web
        ///     bound from the <c>AzureAd</c> configuration section injected by Aspire,
        ///     along with Microsoft Entra Agent Identities support for agent OBO flows.
        /// </summary>
        /// <returns>The same <see cref="IHostApplicationBuilder" /> instance for fluent configuration.</returns>
        public IHostApplicationBuilder AddDefaultAuthentication()
        {
            builder
                .Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            builder.Services.AddAgentIdentities();
            builder
                .Services.AddOptions<AgentIdentityOptions>()
                .BindConfiguration(AgentIdentityOptions.ConfigurationSection);

            return builder;
        }
    }
}
