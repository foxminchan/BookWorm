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
            builder.AddAIAgent(
                Constants.Other.Agents.BookAgent,
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
                    options.AIContextProviders =
                    [
                        skillsProvider,
                        todoProvider,
                        modeProvider,
                        fileMemoryProvider,
                    ];

                    var agent = new ChatClientAgent(chatClient, options);

                    return agent.WithBookWormGovernance(sp, key);
                }
            );
        }
    }
}
