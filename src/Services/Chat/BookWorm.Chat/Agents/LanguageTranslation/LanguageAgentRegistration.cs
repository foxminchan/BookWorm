using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.LanguageTranslation;

internal static class LanguageAgentRegistration
{
    public static void AddLanguageAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            LanguageAgentDefinition.Name,
            (sp, key) =>
            {
                var identityProvider = sp.GetRequiredService<IAgentIdentityProvider>();

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UsePIIMiddleware()
                    .UseGuardrailMiddleware()
                    .UseGovernanceToolCall(identityProvider, LanguageAgentDefinition.Name)
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
                        },
                    }
                );

                return agent;
            }
        );
    }
}
