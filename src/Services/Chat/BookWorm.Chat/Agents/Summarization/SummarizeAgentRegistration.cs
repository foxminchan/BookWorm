using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Governance.IdentityProvider;
using BookWorm.Chassis.AI.Middlewares;
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
                var identityProvider = sp.GetRequiredService<IAgentIdentityProvider>();

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UsePIIMiddleware()
                    .UseGuardrailMiddleware()
                    .UseGovernanceToolCall(identityProvider, SummarizeAgentDefinition.Name)
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
