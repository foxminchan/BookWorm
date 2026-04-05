using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Routing;

internal static class RouterAgentRegistration
{
    public static void AddRouterAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            RouterAgentDefinition.Name,
            (sp, key) =>
            {
                var governanceKernel = sp.GetRequiredService<AgentGovernance.GovernanceKernel>();
                var identityProvider = sp.GetRequiredService<AgentIdentityProvider>();
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(
                        GovernanceToolCallMiddleware.Create(
                            governanceKernel,
                            identityProvider,
                            RouterAgentDefinition.Name
                        ),
                        null
                    )
                    .Use(GuardrailMiddleware.InvokeAsync, null)
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
        );
    }
}
