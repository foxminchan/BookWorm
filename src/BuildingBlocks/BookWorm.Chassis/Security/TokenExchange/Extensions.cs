using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace BookWorm.Chassis.Security.TokenExchange;

public static class Extensions
{
    extension(IHttpClientBuilder builder)
    {
        /// <summary>
        ///     Configures service-to-service token exchange using <c>Microsoft.Identity.Web</c> and
        ///     <see cref="IAuthorizationHeaderProvider" /> for autonomous app-only flows.
        /// </summary>
        /// <param name="serviceKey">Target service identifier used to name the handler configuration.</param>
        /// <returns>The same <see cref="IHttpClientBuilder" /> instance for fluent configuration.</returns>
        public IHttpClientBuilder AddUserTokenExchange(string serviceKey)
        {
            builder.ConfigureScopes(serviceKey);

            return builder.AddMicrosoftIdentityUserAuthenticationHandler(serviceKey, _ => { });
        }

        /// <summary>
        ///     Configures agent token exchange using <c>Microsoft.Identity.Web.AgentIdentities</c> and
        ///     <see cref="IAuthorizationHeaderProvider" /> for either user on-behalf-of (OBO) or autonomous app-only flows.
        /// </summary>
        /// <param name="serviceKey">Target service identifier used to name the handler configuration.</param>
        /// <param name="agentApplicationId">
        ///     Optional Entra Agent Identity application (client) ID. When omitted, resolved from
        ///     <see cref="AgentIdentityOptions" />.
        /// </param>
        /// <param name="isAppOnly">
        ///     When <see langword="true" />, acquires an autonomous app-only token; otherwise performs user OBO exchange.
        /// </param>
        /// <param name="configureOptions">Optional callback to further customize header options.</param>
        /// <returns>The same <see cref="IHttpClientBuilder" /> instance for fluent configuration.</returns>
        public IHttpClientBuilder AddAgentTokenExchange(
            string serviceKey,
            string? agentApplicationId = null,
            bool isAppOnly = false,
            Action<AuthorizationHeaderProviderOptions>? configureOptions = null
        )
        {
            builder.Services.AddHttpContextAccessor();

            builder
                .Services.AddOptions<AgentTokenExchangeOptions>(serviceKey)
                .Configure<
                    IOptionsMonitor<MicrosoftIdentityOptions>,
                    IOptionsMonitor<AgentIdentityOptions>
                >(
                    (options, entraOptions, agentIdentityOptions) =>
                    {
                        var clientId = entraOptions
                            .Get(JwtBearerDefaults.AuthenticationScheme)
                            .ClientId;

                        if (!string.IsNullOrWhiteSpace(clientId))
                        {
                            options.Scopes = [$"api://{clientId}/.default"];
                        }

                        options.AgentApplicationId = !string.IsNullOrWhiteSpace(agentApplicationId)
                            ? agentApplicationId
                            : agentIdentityOptions.CurrentValue.ResolveAgentId(serviceKey);
                        options.IsAppOnly = isAppOnly;
                        options.ConfigureHeaderOptions = configureOptions;
                    }
                );

            return builder.AddHttpMessageHandler(sp => new AgentAuthenticationMessageHandler(
                sp,
                sp.GetRequiredService<IOptionsMonitor<AgentTokenExchangeOptions>>(),
                sp.GetRequiredService<IHttpContextAccessor>(),
                serviceKey
            ));
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
