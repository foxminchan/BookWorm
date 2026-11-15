using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace BookWorm.Chassis.Security.TokenExchange;

public static class Extensions
{
    extension(IHttpClientBuilder builder)
    {
        /// <summary>
        ///     Configures user delegated token exchange using Microsoft.Identity.Web's user authentication handler.
        /// </summary>
        /// <param name="serviceKey">Target service identifier used to name the handler configuration.</param>
        /// <returns>The same <see cref="IHttpClientBuilder" /> instance for fluent configuration.</returns>
        public IHttpClientBuilder AddAuthTokenExchange(string serviceKey)
        {
            builder.ConfigureScopes(serviceKey);

            return builder.AddMicrosoftIdentityUserAuthenticationHandler(serviceKey, _ => { });
        }

        /// <summary>
        ///     Configures application client-credentials token acquisition using Microsoft.Identity.Web's
        ///     application authentication handler.
        /// </summary>
        /// <param name="serviceKey">Target service identifier used to name the handler configuration.</param>
        /// <returns>The same <see cref="IHttpClientBuilder" /> instance for fluent configuration.</returns>
        public IHttpClientBuilder AddClientCredentialsTokenExchange(string serviceKey)
        {
            builder.ConfigureScopes(serviceKey);

            return builder.AddMicrosoftIdentityAppAuthenticationHandler(serviceKey, _ => { });
        }

        private void ConfigureScopes(string serviceKey)
        {
            builder
                .Services.AddOptions<MicrosoftIdentityAuthenticationMessageHandlerOptions>(
                    serviceKey
                )
                .Configure<IOptionsMonitor<MicrosoftIdentityOptions>>(
                    (options, entraOptions) =>
                    {
                        var clientId = entraOptions
                            .Get(JwtBearerDefaults.AuthenticationScheme)
                            .ClientId;

                        if (!string.IsNullOrWhiteSpace(clientId))
                        {
                            options.Scopes = $"api://{clientId}/.default";
                        }
                    }
                );
        }
    }
}
