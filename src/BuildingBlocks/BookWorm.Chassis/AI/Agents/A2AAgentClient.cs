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
        var (card, httpClient) = ResolveClient(agentName);

        await HandleAuthenticationAsync(agentClientId, scope, httpClient, serviceProvider);

        return card.AsAIAgent(httpClient);
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

    private (AgentCard, HttpClient) ResolveClient(string agentName)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new($"{baseUri}"),
            Timeout = TimeSpan.FromMinutes(2),
        };

        var endpointUrl = new Uri(httpClient.BaseAddress!, $"/{path?.TrimStart('/')}/{agentName}");

        var card = new AgentCard
        {
            Name = agentName,
            SupportedInterfaces =
            [
                new()
                {
                    Url = endpointUrl.ToString(),
                    ProtocolBinding = ProtocolBindingNames.JsonRpc,
                },
            ],
        };

        return (card, httpClient);
    }
}
