using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Governance.IdentityProvider;
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

    public static void AddBookAgent(this IHostApplicationBuilder builder)
    {
        builder.AddAIAgent(
            BookAgentDefinition.Name,
            (sp, key) =>
            {
                var identityProvider = sp.GetRequiredService<IAgentIdentityProvider>();

                var compactionProvider = CompactionPipelineFactory.CreateFull(
                    sp.GetRequiredService<IChatClient>()
                );

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UsePIIMiddleware()
                    .UseGuardrailMiddleware()
                    .UseGovernanceToolCall(identityProvider, BookAgentDefinition.Name)
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
                    scriptRunner: null,
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
