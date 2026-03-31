using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.CustomerSupport;

internal static class QAAgentRegistration
{
    public static void AddQAAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            QAAgentDefinition.Name,
            (sp, key) =>
            {
                var presidioService = sp.GetRequiredService<IPresidioService>();
                var compactionProvider = CompactionPipelineFactory.CreateLight();
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.Create(presidioService), null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .UseAIContextProviders(compactionProvider)
                    .Build(sp);

                var skillsProvider = new AgentSkillsProvider(
                    Path.Combine(AppContext.BaseDirectory, "Skills", "store-policies"),
                    scriptRunner: null,
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
