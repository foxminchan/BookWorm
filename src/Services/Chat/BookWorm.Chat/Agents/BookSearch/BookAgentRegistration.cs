using Azure.AI.Projects;
using BookWorm.Chat.Orchestration;
using Microsoft.Agents.AI.Foundry;

namespace BookWorm.Chat.Agents.BookSearch;

internal static class BookAgentRegistration
{
    private const string DefinitionFile = "Agents/BookSearch/book-agent.yaml";

    private const string CatalogResearcherDefinitionFile =
        "Agents/BookSearch/catalog-researcher.yaml";

    private const string RecommendationResearcherDefinitionFile =
        "Agents/BookSearch/recommendation-researcher.yaml";

    private const string FileMemoryRootConfigKey = "Chat:Harness:FileMemory:Root";

    extension(IHostApplicationBuilder builder)
    {
        public void AddBookAgent()
        {
            builder.Services.AddSingleton(sp =>
                HarnessProviderFactory.CreateFoundryMemoryProvider(
                    sp.GetRequiredService<AIProjectClient>(),
                    sp.GetRequiredService<IConfiguration>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    sp.GetService<ILoggerFactory>()
                )
            );

            builder.AddAIAgent(Constants.Other.Agents.BookAgent, (sp, key) =>
            {
                var compactionProvider = CompactionPipelineFactory.CreateFull(
                    sp.GetRequiredService<IChatClient>()
                );

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UseAGUIClientContext()
                    .UseDecisionToolSelection(
                        sp,
                        HarnessProviderFactory.CreateBookAgentToolSelectionOptions()
                    )
                    .UsePIIMiddleware(sp)
                    .UseGuardrailMiddleware()
                    .UseAIContextProviders(compactionProvider)
                    .Build(sp);

                var mcpClient = sp.GetRequiredService<McpClient>();
                var skillsProvider = mcpClient.CreateSkillsProvider(
                    new HashSet<string>(["book-catalog"], StringComparer.OrdinalIgnoreCase),
                    sp.GetService<ILoggerFactory>()
                );

                var fileMemoryRoot = sp.GetRequiredService<IConfiguration>()
                    .GetValue(
                        FileMemoryRootConfigKey,
                        Path.Combine(AppContext.BaseDirectory, "agent-files")
                    );

                var (todoProvider, modeProvider) = HarnessProviderFactory.CreatePlanningProviders();

                var fileMemoryProvider = HarnessProviderFactory.CreateFileMemoryProvider(
                    fileMemoryRoot
                );

                var (options, toolNames) = AgentDefinitionLoader.Load(DefinitionFile, key);
                var chatOptions = options.ChatOptions;
                var catalogTools = mcpClient.GetAgentTools(toolNames);
                var catalogResearcher = chatClient.InitSubAgent(
                    sp,
                    mcpClient,
                    CatalogResearcherDefinitionFile,
                    "book-catalog-researcher"
                );

                var recommendationResearcher = chatClient.InitSubAgent(
                    sp,
                    mcpClient,
                    RecommendationResearcherDefinitionFile,
                    "book-recommendation-researcher"
                );

                chatOptions?.Tools = [
                    .. catalogTools,
                    .. SubAgentFactory.Create(catalogResearcher, recommendationResearcher),
                ];
                options.AllowConcurrentInvocation = true;

                List<AIContextProvider> contextProviders =
                [
                    skillsProvider,
                    todoProvider,
                    modeProvider,
                    fileMemoryProvider,
                ];

                if (sp.GetService<FoundryMemoryProvider>() is { } foundryMemoryProvider)
                {
                    contextProviders.Add(foundryMemoryProvider);
                }

                options.AIContextProviders = contextProviders;

                var agent = new ChatClientAgent(chatClient, options);

                return agent.WithBookWormGovernance(sp, key);
            });
        }
    }
}
