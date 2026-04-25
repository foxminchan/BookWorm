using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.BookSearch;

internal static class BookAgentRegistration
{
    private static readonly string[] _catalogToolNames =
    [
        "search_catalog",
        "get_book",
        "list_categories",
        "list_authors",
    ];

    extension(IHostApplicationBuilder builder)
    {
        public void AddBookAgent()
        {
            builder.AddAIAgent(
                BookAgentDefinition.Name,
                (sp, key) =>
                {
                    var compactionProvider = CompactionPipelineFactory.CreateFull(
                        sp.GetRequiredService<IChatClient>()
                    );

                    var chatClient = sp.GetRequiredService<IChatClient>()
                        .AsBuilder()
                        .UsePIIMiddleware(sp)
                        .UseGuardrailMiddleware()
                        .UseGovernanceToolCall(sp, BookAgentDefinition.Name)
                        .UseAIContextProviders(compactionProvider)
                        .Build(sp);

                    var mcpClient = sp.GetRequiredService<McpClient>();
                    var mcpTools = mcpClient
                        .ListToolsAsync()
                        .Preserve()
                        .GetAwaiter()
                        .GetResult()
                        .Where(t => _catalogToolNames.Contains(t.Name))
                        .Cast<AITool>()
                        .ToArray();

                    var skillsProvider = new AgentSkillsProvider(
                        Path.Combine(AppContext.BaseDirectory, "Skills", "book-catalog"),
                        loggerFactory: sp.GetService<ILoggerFactory>()
                    );

                    var agent = new ChatClientAgent(
                        chatClient,
                        options: new()
                        {
                            Name = key,
                            Description = BookAgentDefinition.Description,
                            AIContextProviders = [skillsProvider],
                            ChatOptions = new()
                            {
                                Instructions = BookAgentDefinition.Instructions,
                                Temperature = 0.7f,
                                MaxOutputTokens = 2000,
                                TopP = 0.95f,
                                AllowMultipleToolCalls = true,
                                Tools = mcpTools,
                            },
                        }
                    );

                    return agent;
                }
            );
        }
    }
}
