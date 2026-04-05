using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
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
        var services = builder.Services;

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Services.Rating)
                    .Build()
            )
        );

        builder.AddAIAgent(
            BookAgentDefinition.Name,
            (sp, key) =>
            {
                var presidioService = sp.GetRequiredService<IPresidioService>();
                var governanceKernel = sp.GetRequiredService<AgentGovernance.GovernanceKernel>();
                var identityProvider = sp.GetRequiredService<AgentIdentityProvider>();
                var compactionProvider = CompactionPipelineFactory.CreateFull(
                    sp.GetRequiredService<IChatClient>()
                );
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.Create(presidioService), null)
                    .Use(
                        GovernanceToolCallMiddleware.Create(
                            governanceKernel,
                            identityProvider,
                            BookAgentDefinition.Name
                        ),
                        null
                    )
                    .Use(GuardrailMiddleware.InvokeAsync, null)
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
