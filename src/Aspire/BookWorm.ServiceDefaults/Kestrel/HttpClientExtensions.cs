namespace BookWorm.ServiceDefaults.Kestrel;

public static class HttpClientExtensions
{
    extension(IHttpClientBuilder builder)
    {
        /// <summary>
        ///     Adds a delegating handler that forwards the current user access token
        ///     from the active HTTP context to outgoing HTTP client requests.
        /// </summary>
        /// <returns>
        ///     The configured <see cref="IHttpClientBuilder" /> instance for chaining.
        /// </returns>
        public IHttpClientBuilder AddAuthToken()
        {
            builder.Services.TryAddTransient<HttpClientAuthorizationDelegatingHandler>();

            builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            return builder;
        }
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
                request.Headers.Authorization = new(
                    JwtBearerDefaults.AuthenticationScheme,
                    accessToken
                );
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
