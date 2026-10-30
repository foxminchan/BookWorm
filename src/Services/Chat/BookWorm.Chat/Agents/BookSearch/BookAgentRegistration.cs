using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
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
                    options.AIContextProviders =
                    [
                        skillsProvider,
                        todoProvider,
                        modeProvider,
                        fileMemoryProvider,
                    ];
                    options.ChatOptions!.Reasoning = new() { Effort = ReasoningEffort.High };

                    var agent = new ChatClientAgent(chatClient, options);

                    return agent
                        .WithBookWormGovernance(sp, key);
                }
            );
        }
    }
}
