using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Security.TokenExchange;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the token-exchange service.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection" /> instance.</returns>
        private void AddTokenExchange()
        {
            services.TryAddTransient<ITokenExchange, TokenExchange>();
        }
    }

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers Keycloak token exchange services without enabling inbound authentication.
        /// </summary>
        /// <returns>The same <see cref="IHostApplicationBuilder" /> instance.</returns>
        public IHostApplicationBuilder AddKeycloakTokenExchange()
        {
            builder.Configure<IdentityOptions>(IdentityOptions.ConfigurationSection);
            builder.AddKeycloakClient();

            builder.Services.AddTokenExchange();
            return builder;
        }
    }

    extension(IHttpClientBuilder builder)
    {
        /// <summary>
        ///     Adds token exchange support to the configured <see cref="IHttpClientBuilder" /> by
        ///     registering the token exchange service and an authorization delegating handler.
        /// </summary>
        /// <param name="serviceKey">
        ///     Optional target service identifier used to resolve audience and scope for token exchange.
        /// </param>
        /// <returns>
        ///     The same <see cref="IHttpClientBuilder" /> instance for fluent configuration.
        /// </returns>
        public IHttpClientBuilder AddAuthTokenExchange(string? serviceKey = null)
        {
            var service = builder.Services;

            service.AddTokenExchange();

            builder.AddHttpMessageHandler(sp => new HttpClientAuthorizationDelegatingHandler(
                sp.GetRequiredService<IHttpContextAccessor>(),
                serviceKey
            ));

            return builder;
        }
    }

    private sealed class HttpClientAuthorizationDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        string? serviceKey
    ) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (httpContextAccessor.HttpContext is not { } context)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var identityOptions = context.RequestServices.GetService<IdentityOptions>();

            var (audience, scope) = ResolveTokenExchangeTarget(identityOptions, serviceKey);

            var tokenExchange = context.RequestServices.GetRequiredService<ITokenExchange>();

            var exchangedToken = await tokenExchange.ExchangeAsync(
                context.User,
                audience,
                scope,
                cancellationToken
            );

            request.Headers.Authorization = new(
                JwtBearerDefaults.AuthenticationScheme,
                exchangedToken
            );

            return await base.SendAsync(request, cancellationToken);
        }

        private static (string? Audience, string? Scope) ResolveTokenExchangeTarget(
            IdentityOptions? identity,
            string? serviceKey
        )
        {
            if (
                string.IsNullOrWhiteSpace(serviceKey)
                || identity?.TokenExchangeTargets is null
                || identity.TokenExchangeTargets.Count == 0
            )
            {
                return (null, null);
            }

            if (identity.TokenExchangeTargets.TryGetValue(serviceKey, out var scope))
            {
                return (serviceKey, scope);
            }

            // fallback: find a configured key that is contained in the service key
            var found = identity.TokenExchangeTargets.FirstOrDefault(kvp =>
                !string.IsNullOrWhiteSpace(kvp.Key)
                && (
                    serviceKey.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)
                    || kvp.Key.Contains(serviceKey, StringComparison.OrdinalIgnoreCase)
                )
            );

            return !string.IsNullOrWhiteSpace(found.Key) ? (found.Key, found.Value) : (null, null);
        }
    }
}
