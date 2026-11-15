using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace BookWorm.Chassis.Security.TokenExchange;

internal sealed class AgentAuthenticationMessageHandler(
    IServiceProvider serviceProvider,
    IOptionsMonitor<AgentTokenExchangeOptions> optionsMonitor,
    IHttpContextAccessor httpContextAccessor,
    string serviceKey
) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var exchangeOptions = optionsMonitor.Get(serviceKey);

        if (exchangeOptions.Scopes.Length <= 0)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var provider = (
            httpContextAccessor.HttpContext?.RequestServices ?? serviceProvider
        ).GetRequiredService<IAuthorizationHeaderProvider>();

        AuthorizationHeaderProviderOptions headerOptions = new();

        if (!string.IsNullOrWhiteSpace(exchangeOptions.AgentApplicationId))
        {
            headerOptions.WithAgentIdentity(exchangeOptions.AgentApplicationId);
        }

        exchangeOptions.ConfigureHeaderOptions?.Invoke(headerOptions);

        var authorizationHeader = exchangeOptions.IsAppOnly
            ? await provider.CreateAuthorizationHeaderForAppAsync(
                exchangeOptions.Scopes[0],
                headerOptions,
                cancellationToken
            )
            : await provider.CreateAuthorizationHeaderForUserAsync(
                exchangeOptions.Scopes,
                headerOptions,
                httpContextAccessor.HttpContext?.User,
                cancellationToken
            );

        request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);

        return await base.SendAsync(request, cancellationToken);
    }
}
