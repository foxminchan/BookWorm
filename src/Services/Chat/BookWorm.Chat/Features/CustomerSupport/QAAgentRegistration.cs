using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Features.CustomerSupport;

internal static class QAAgentRegistration
{
    public static void AddQAAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            QAAgentDefinition.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var skillsProvider = new FileAgentSkillsProvider(
                    Path.Combine(AppContext.BaseDirectory, "skills", "store-policies"),
                    loggerFactory: sp.GetService<ILoggerFactory>()
                );

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Description = QAAgentDefinition.Description,
                        AIContextProviders = [skillsProvider],
                        ChatOptions = new()
                        {
                            Instructions = QAAgentDefinition.Instructions,
                            Temperature = 0.5f,
                            MaxOutputTokens = 1000,
                        },
                    }
                );

                return agent;
            }
        );
    }
}
