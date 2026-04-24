using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Routing;

internal static class RouterAgentRegistration
{
    public static void AddRouterAgent(this IHostApplicationBuilder builder)
    {
        builder
            .AddAIAgent(
                RouterAgentDefinition.Name,
                (sp, key) =>
                {
                    var chatClient = sp.GetRequiredService<IChatClient>()
                        .AsBuilder()
                        .UseGuardrailMiddleware()
                        .UseGovernanceToolCall(sp, RouterAgentDefinition.Name)
                        .Build(sp);

                    var agent = new ChatClientAgent(
                        chatClient,
                        options: new()
                        {
                            Name = key,
                            Description = RouterAgentDefinition.Description,
                            ChatOptions = new()
                            {
                                Instructions = RouterAgentDefinition.Instructions,
                                Temperature = 0.1f,
                                MaxOutputTokens = 200,
                            },
                        }
                    );

                    return agent;
                }
            )
            .AddA2AServer();
    }
}
