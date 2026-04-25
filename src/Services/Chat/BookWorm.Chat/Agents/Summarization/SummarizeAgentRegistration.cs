using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Summarization;

internal static class SummarizeAgentRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddSummarizeAgent()
        {
            builder
                .AddAIAgent(
                    SummarizeAgentDefinition.Name,
                    (sp, key) =>
                    {
                        var chatClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UsePIIMiddleware(sp)
                            .UseGuardrailMiddleware()
                            .UseGovernanceToolCall(sp, SummarizeAgentDefinition.Name)
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
                )
                .AddA2AServer();
        }
    }
}
