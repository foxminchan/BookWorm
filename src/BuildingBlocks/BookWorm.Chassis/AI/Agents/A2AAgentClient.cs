using System.Security.Claims;
using A2A;
using BookWorm.Chassis.Security.TokenExchange;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Agents;

public sealed class A2AAgentClient(Uri baseUri, string? path)
{
    public async Task<AIAgent> GetAIAgent(IServiceProvider serviceProvider, string agentName)
    {
        var (resolver, httpClient) = ResolveClient(agentName);

        var agentCard = await resolver.GetAgentCardAsync();

        await HandleAuthenticationAsync(agentName, agentCard, httpClient, serviceProvider);

        var agent = await resolver.GetAIAgentAsync(httpClient);

        return agent;
    }

    private static async Task HandleAuthenticationAsync(
        string agentName,
        AgentCard agentCard,
        HttpClient httpClient,
        IServiceProvider serviceProvider
    )
    {
        if (
            agentCard.SecuritySchemes is not null
            && agentCard.SecuritySchemes.TryGetValue(
                OAuthDefaults.DisplayName,
                out var securityScheme
            )
            && securityScheme is OAuth2SecurityScheme oAuth2SecurityScheme
        )
        {
            var scope = string.Join(
                ' ',
                oAuth2SecurityScheme.Flows.AuthorizationCode?.Scopes.Keys ?? []
            );

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new InvalidOperationException(
                    $"Agent '{agentName}' JWT Bearer configuration is invalid: no scopes defined."
                );
            }

            var tokenExchange = serviceProvider.GetRequiredService<ITokenExchange>();
            var claimsPrincipal = serviceProvider.GetRequiredService<ClaimsPrincipal>();

            var accessToken = await tokenExchange.ExchangeAsync(claimsPrincipal, scope: scope);

            httpClient.DefaultRequestHeaders.Authorization = new(
                JwtBearerDefaults.AuthenticationScheme,
                accessToken
            );
        }
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
            $"/{path?.TrimStart('/')}/{agentName}/v1/card/"
        );

        return (resolver, httpClient);
    }
}
