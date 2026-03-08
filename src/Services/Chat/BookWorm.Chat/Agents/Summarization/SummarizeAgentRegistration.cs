using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Summarization;

internal static class SummarizeAgentRegistration
{
    public static void AddSummarizeAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            SummarizeAgentDefinition.Name,
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
                        Description = SummarizeAgentDefinition.Description,
                        ChatOptions = new()
                        {
                            Instructions = SummarizeAgentDefinition.Instructions,
                            Temperature = 0.4f,
                            MaxOutputTokens = 800,
                        },
                    }
                );

                return agent;
            }
        );
    }
}
