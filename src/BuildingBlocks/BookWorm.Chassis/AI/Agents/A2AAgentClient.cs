using System.Security.Claims;
using A2A;
using A2A.Grpc;
using BookWorm.Chassis.Security.TokenExchange;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Agents;

internal sealed class A2AAgentClient(Uri baseUri, string? path)
{
    static A2AAgentClient()
    {
        A2AGrpcClientRegistration.Register();
    }

    public async Task<AIAgent> GetAIAgent(
        IServiceProvider serviceProvider,
        string agentName,
        string? agentClientId,
        string? scope,
        CancellationToken cancellationToken = default
    )
    {
        var (card, httpClient) = ResolveClient(agentName);

        await HandleAuthenticationAsync(
            agentClientId,
            scope,
            httpClient,
            serviceProvider,
            cancellationToken
        );

        return card.AsAIAgent(
            httpClient,
            new()
            {
                PreferredBindings =
                [
                    ProtocolBindingNames.Grpc,
                    ProtocolBindingNames.JsonRpc,
                    ProtocolBindingNames.HttpJson,
                ],
            }
        );
    }

    private static async Task HandleAuthenticationAsync(
        string? agentClientId,
        string? scope,
        HttpClient httpClient,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(agentClientId) || string.IsNullOrWhiteSpace(scope))
        {
            return;
        }

        var tokenExchange = serviceProvider.GetRequiredService<ITokenExchange>();
        var claimsPrincipal = serviceProvider.GetRequiredService<ClaimsPrincipal>();

        var accessToken = await tokenExchange.ExchangeAsync(
            claimsPrincipal,
            agentClientId,
            scope,
            cancellationToken
        );

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

        var endpointUrl = new Uri(httpClient.BaseAddress, $"/{path?.TrimStart('/')}/{agentName}");

        var card = new AgentCard
        {
            Name = agentName,
            SupportedInterfaces =
            [
                new() { Url = endpointUrl.ToString(), ProtocolBinding = ProtocolBindingNames.Grpc },
                new()
                {
                    Url = endpointUrl.ToString(),
                    ProtocolBinding = ProtocolBindingNames.JsonRpc,
                },
                new()
                {
                    Url = endpointUrl.ToString(),
                    ProtocolBinding = ProtocolBindingNames.HttpJson,
                },
            ],
        };

        return (card, httpClient);
    }
}
