using System.Security.Claims;
using A2A;
using BookWorm.Chassis.Security.TokenExchange;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Agents;

internal sealed class A2AAgentClient(Uri baseUri, string? path)
{
    public async Task<AIAgent> GetAIAgent(
        IServiceProvider serviceProvider,
        string agentName,
        string? agentClientId,
        string? scope
    )
    {
        var (resolver, httpClient) = ResolveClient(agentName);

        await HandleAuthenticationAsync(agentClientId, scope, httpClient, serviceProvider);

        var agent = await resolver.GetAIAgentAsync(httpClient);

        return agent;
    }

    private static async Task HandleAuthenticationAsync(
        string? agentClientId,
        string? scope,
        HttpClient httpClient,
        IServiceProvider serviceProvider
    )
    {
        if (string.IsNullOrWhiteSpace(agentClientId) || string.IsNullOrWhiteSpace(scope))
        {
            return;
        }

        var tokenExchange = serviceProvider.GetRequiredService<ITokenExchange>();
        var claimsPrincipal = serviceProvider.GetRequiredService<ClaimsPrincipal>();

        var accessToken = await tokenExchange.ExchangeAsync(claimsPrincipal, agentClientId, scope);

        httpClient.DefaultRequestHeaders.Authorization = new(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken
        );
    }

    private (A2ACardResolver, HttpClient) ResolveClient(string agentName)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new($"{baseUri}"),
            Timeout = TimeSpan.FromMinutes(2),
        };

        var resolver = new A2ACardResolver(
            httpClient.BaseAddress,
            httpClient,
            $"/{path?.TrimStart('/')}/{agentName}/card"
        );

        return (resolver, httpClient);
    }
}
