using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.SentimentAnalysis;

internal static class SentimentAgentRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddSentimentAgent()
        {
            builder
                .AddAIAgent(
                    SentimentAgentDefinition.Name,
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
                                Description = SentimentAgentDefinition.Description,
                                ChatOptions = new()
                                {
                                    Instructions = SentimentAgentDefinition.Instructions,
                                    Temperature = 0.2f,
                                    MaxOutputTokens = 300,
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
