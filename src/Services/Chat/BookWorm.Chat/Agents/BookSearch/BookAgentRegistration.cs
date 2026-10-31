using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chat.Memory;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.BookSearch;

internal static class BookAgentRegistration
{
    private const string DefinitionFile = "Agents/BookSearch/book-agent.yaml";
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
                    var chatOptions = options.ChatOptions!;
                    chatOptions.Tools = mcpClient.GetAgentTools(toolNames);
                    options.AllowConcurrentInvocation = true;

                    // Durable cross-session memory is optional: it is only registered when its
                    // backing stores (chatdb, Qdrant) are configured.
                    var userMemoryProvider = sp.GetService<UserMemoryContextProvider>();

                    options.AIContextProviders = userMemoryProvider is null
                        ? [skillsProvider, todoProvider, modeProvider, fileMemoryProvider]
                        :
                        [
                            skillsProvider,
                            todoProvider,
                            modeProvider,
                            fileMemoryProvider,
                            userMemoryProvider,
                        ];

                    var agent = new ChatClientAgent(chatClient, options);

                    return agent.WithBookWormGovernance(sp, key);
                }
            );
        }
    }
}
