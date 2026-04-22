using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Basket;

internal static class BasketAgentRegistration
{
    public static void AddBasketAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            BasketAgentDefinition.Name,
            (sp, key) =>
            {
                var compactionProvider = CompactionPipelineFactory.CreateLight();

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UsePIIMiddleware(sp)
                    .UseGuardrailMiddleware()
                    .UseGovernanceToolCall(sp, BasketAgentDefinition.Name)
                    .UseAIContextProviders(compactionProvider)
                    .Build(sp);

                // The basket actions (`addToBasket`, `viewBasket`) are exposed as
                // AG-UI / CopilotKit client-side tools by the storefront. They are
                // forwarded into the agent's tool list at request time by the AG-UI
                // hosting bridge, so we deliberately do not register any server-side
                // (MCP) tools here. The agent simply needs to know the tool names —
                // that's covered in BasketAgentDefinition.Instructions.
                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Description = BasketAgentDefinition.Description,
                        ChatOptions = new()
                        {
                            Instructions = BasketAgentDefinition.Instructions,
                            Temperature = 0.3f,
                            MaxOutputTokens = 800,
                            AllowMultipleToolCalls = true,
                        },
                    }
                );

                return agent;
            }
        );
    }
}
