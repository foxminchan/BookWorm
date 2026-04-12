using BookWorm.Chassis.AI.Governance.IdentityProvider;
using BookWorm.Chassis.AI.Middlewares;
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
                var identityProvider = sp.GetRequiredService<IAgentIdentityProvider>();

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UsePIIMiddleware()
                    .UseGuardrailMiddleware()
                    .UseGovernanceToolCall(identityProvider, SentimentAgentDefinition.Name)
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
