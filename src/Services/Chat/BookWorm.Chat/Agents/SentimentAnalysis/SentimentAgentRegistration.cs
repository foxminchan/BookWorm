using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.SentimentAnalysis;

internal static class SentimentAgentRegistration
{
    public static void AddSentimentAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            SentimentAgentDefinition.Name,
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
                        Description = SentimentAgentDefinition.Description,
                        ChatOptions = new()
                        {
                            Instructions = SentimentAgentDefinition.Instructions,
                            Temperature = 0.2f,
                            MaxOutputTokens = 300,
                        },
                    }
                );

                return agent;
            }
        );
    }
}
