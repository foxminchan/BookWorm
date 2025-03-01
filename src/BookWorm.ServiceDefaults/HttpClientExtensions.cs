using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookWorm.ServiceDefaults;

public static class HttpClientExtensions
{
    public static IHttpClientBuilder AddAuthToken(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddTransient<HttpClientAuthorizationDelegatingHandler>();

        builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

        return builder;
    }

    private sealed class HttpClientAuthorizationDelegatingHandler(
        IHttpContextAccessor httpContextAccessor
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

            if (accessToken is not null)
            {
                request.Headers.Authorization = new("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
