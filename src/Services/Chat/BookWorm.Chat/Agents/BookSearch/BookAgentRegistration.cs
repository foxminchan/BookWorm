using BookWorm.Chassis.AI.Governance;
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

    private const string FileMemoryRootConfigKey = "Chat:Harness:FileMemory:Root";

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

                    var fileMemoryRoot = sp.GetRequiredService<IConfiguration>()
                        .GetValue(
                            FileMemoryRootConfigKey,
                            Path.Combine(AppContext.BaseDirectory, "agent-files")
                        );

                    var (todoProvider, modeProvider) =
                        HarnessProviderFactory.CreatePlanningProviders();
                    var fileMemoryProvider = HarnessProviderFactory.CreateFileMemoryProvider(
                        fileMemoryRoot
                    );

                    var agent = new ChatClientAgent(
                        chatClient,
                        options: new()
                        {
                            Name = key,
                            Description = BookAgentDefinition.Description,
                            AIContextProviders =
                            [
                                skillsProvider,
                                todoProvider,
                                modeProvider,
                                fileMemoryProvider,
                            ],
                            ChatOptions = new()
                            {
                                Instructions = BookAgentDefinition.Instructions,
                                Temperature = 0.7f,
                                MaxOutputTokens = 2000,
                                TopP = 0.95f,
                                AllowMultipleToolCalls = true,
                                Tools = mcpTools,
                                Reasoning = new() { Effort = ReasoningEffort.High },
                            },
                        }
                    );

                    return agent.WithBookWormGovernance(sp, key);
                }
            );
        }
    }
}
