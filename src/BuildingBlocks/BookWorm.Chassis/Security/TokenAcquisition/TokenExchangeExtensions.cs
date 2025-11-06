using BookWorm.Chassis.Security.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookWorm.Chassis.Security.TokenAcquisition;

public static class TokenExchangeExtensions
{
    public static IHttpClientBuilder AddAuthTokenAcquisition(
        this IHttpClientBuilder builder,
        string? audience = null,
        string? scope = null
    )
    {
        builder.Services.TryAddTransient<ITokenExchange, TokenExchange>();

        builder.Services.AddTransient(sp => new HttpClientAuthorizationDelegatingHandler(
            sp.GetRequiredService<IHttpContextAccessor>(),
            audience,
            scope
        ));

        builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

        return builder;
    }

    public static (string? Audience, string? Scope) ResolveTokenExchangeTarget(
        this IdentityOptions? identity,
        string serviceKey
    )
    {
        if (identity?.TokenExchangeTargets is null || identity.TokenExchangeTargets.Count == 0)
        {
            return (null, null);
        }

        // exact key match
        if (identity.TokenExchangeTargets.TryGetValue(serviceKey, out var s))
        {
            return (serviceKey, s);
        }

        // fallback: find a configured key that is contained in the service key or vice versa
        var found = identity.TokenExchangeTargets.FirstOrDefault(kvp =>
            !string.IsNullOrWhiteSpace(kvp.Key)
            && (
                serviceKey.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)
                || kvp.Key.Contains(serviceKey, StringComparison.OrdinalIgnoreCase)
            )
        );

        return !string.IsNullOrWhiteSpace(found.Key) ? (found.Key, found.Value) : (null, null);
    }

    private sealed class HttpClientAuthorizationDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        string? audience,
        string? scope
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

            var tokenExchange = context.RequestServices.GetRequiredService<ITokenExchange>();

            var exchangedToken = await tokenExchange.ExchangeAsync(
                accessToken,
                audience,
                scope,
                cancellationToken
            );

            request.Headers.Authorization = new(
                JwtBearerDefaults.AuthenticationScheme,
                exchangedToken.AccessToken
            );

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
