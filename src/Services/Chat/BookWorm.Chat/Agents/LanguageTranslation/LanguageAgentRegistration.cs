using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.LanguageTranslation;

internal static class LanguageAgentRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddLanguageAgent()
        {
            builder
                .AddAIAgent(
                    LanguageAgentDefinition.Name,
                    (sp, key) =>
                    {
                        var chatClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UsePIIMiddleware(sp)
                            .UseGuardrailMiddleware()
                            .Build(sp);

                        var agent = new ChatClientAgent(
                            chatClient,
                            options: new()
                            {
                                Name = key,
                                Description = LanguageAgentDefinition.Description,
                                ChatOptions = new()
                                {
                                    Instructions = LanguageAgentDefinition.Instructions,
                                    Temperature = 0.3f,
                                    MaxOutputTokens = 500,
                                    Reasoning = new() { Effort = ReasoningEffort.Low },
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
