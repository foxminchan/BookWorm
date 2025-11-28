using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Core;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

namespace BookWorm.Basket.Infrastructure.Agents;

public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddAIServices().WithAITelemetry();

        services.AddOpenAIResponses();
        services.AddOpenAIConversations();
        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Services.Chatting)
                    .Build()
            )
        );

        builder.AddAIAgent(
            BasketAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = BasketAgent.Instructions,
                        Description = BasketAgent.Description,
                        ChatOptions = new() { Temperature = 0.2f, MaxOutputTokens = 800 },
                    }
                );

                return agent;
            }
        );
    }

    public static void MapAgentsDiscovery(this WebApplication app)
    {
        app.MapAgentDiscovery("/agents");

        // Map A2A
        app.MapA2A(BasketAgent.Name, $"/a2a/{BasketAgent.Name}", BasketAgent.AgentCard)
            .WithTags(nameof(BasketAgent))
            .RequireAuthorization();
    }
}
