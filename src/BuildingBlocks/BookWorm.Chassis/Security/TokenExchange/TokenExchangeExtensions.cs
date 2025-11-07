using BookWorm.Chassis.Security.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookWorm.Chassis.Security.TokenExchange;

public static class TokenExchangeExtensions
{
    public static IHttpClientBuilder AddAuthTokenExchange(
        this IHttpClientBuilder builder,
        string? serviceKey = null
    )
    {
        var service = builder.Services;

        service.TryAddTransient<ITokenExchange, TokenExchange>();

        service.AddTransient(sp => new HttpClientAuthorizationDelegatingHandler(
            sp.GetRequiredService<IHttpContextAccessor>(),
            serviceKey
        ));

        builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

        return builder;
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

            var accessToken = await context.GetTokenAsync("access_token");

            if (accessToken is null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var identityOptions = context.RequestServices.GetService<IdentityOptions>();

            var (audience, scope) = ResolveTokenExchangeTarget(identityOptions, serviceKey);

            var tokenExchange = context.RequestServices.GetRequiredService<ITokenExchange>();

            var exchangedToken = await tokenExchange.ExchangeAsync(
                accessToken,
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
