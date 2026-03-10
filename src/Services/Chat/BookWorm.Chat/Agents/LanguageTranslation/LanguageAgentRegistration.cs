using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
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
                var presidioService = sp.GetRequiredService<IPresidioService>();
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.Create(presidioService), null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
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
