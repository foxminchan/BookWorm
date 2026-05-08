using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Routing;

internal static class RouterAgentRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddRouterAgent()
        {
            builder
                .AddAIAgent(
                    RouterAgentDefinition.Name,
                    (sp, key) =>
                    {
                        var chatClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UseGuardrailMiddleware()
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

                        return agent.WithBookWormGovernance(sp, key);
                    }
                )
                .AddA2AServer();
        }
    }
}
