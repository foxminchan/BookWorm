using System.Security.Claims;
using A2A;
using BookWorm.Chassis.Security.TokenExchange;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Chassis.AI.Agents;

public sealed class A2AAgentClient(Uri baseUri, string? path)
{
    public async Task<AIAgent> GetAIAgent(
        string agentName,
        ITokenExchange? tokenExchange = null,
        ClaimsPrincipal? claimsPrincipal = null
    )
    {
        var (resolver, httpClient) = ResolveClient(agentName);

        var agentCard = await resolver.GetAgentCardAsync();

        if (tokenExchange is not null)
        {
            await HandleAuthenticationAsync(
                agentName,
                agentCard,
                httpClient,
                tokenExchange,
                claimsPrincipal
            );
        }

        var agent = await resolver.GetAIAgentAsync(httpClient);

        return agent;
    }

    private static async Task HandleAuthenticationAsync(
        string agentName,
        AgentCard agentCard,
        HttpClient httpClient,
        ITokenExchange tokenExchange,
        ClaimsPrincipal? claimsPrincipal
    )
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        if (
            agentCard.SecuritySchemes is null
            || !agentCard.SecuritySchemes.TryGetValue(
                OAuthDefaults.DisplayName,
                out var securityScheme
            )
            || securityScheme is not OAuth2SecurityScheme oauth2Scheme
        )
        {
            throw new InvalidOperationException(
                $"Agent '{agentName}' does not support OAuth2 authentication."
            );
        }

        var scope = string.Join(' ', oauth2Scheme.Flows.AuthorizationCode?.Scopes.Keys ?? []);

        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new InvalidOperationException(
                $"Agent '{agentName}' OAuth2 configuration is invalid: no scopes defined."
            );
        }

        var accessToken = await tokenExchange.ExchangeAsync(claimsPrincipal, scope: scope);

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
            $"/{path?.TrimStart('/')}/{agentName}/v1/card/"
        );

        return (resolver, httpClient);
    }
}
