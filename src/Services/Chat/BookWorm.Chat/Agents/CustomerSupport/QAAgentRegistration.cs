using BookWorm.Chassis.Caching.Semantic;
using BookWorm.Chat.Orchestration;

namespace BookWorm.Chat.Agents.CustomerSupport;

internal static class QAAgentRegistration
{
    private const string DefinitionFile = "Agents/CustomerSupport/qa-agent.yaml";

    extension(IHostApplicationBuilder builder)
    {
        public void AddQAAgent()
        {
            builder.AddAIAgent(Constants.Other.Agents.QAAgent, (sp, key) =>
            {
                var compactionProvider = CompactionPipelineFactory.CreateLight();

                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UseAGUIClientContext()
                    .UseDecisionToolSelection(
                        sp,
                        HarnessProviderFactory.CreateQAAgentToolSelectionOptions()
                    )
                    .UsePIIMiddleware(sp)
                    .UseGuardrailMiddleware()
                    .UseSemanticCache()
                    .UseAIContextProviders(compactionProvider)
                    .Build(sp);

                var mcpClient = sp.GetRequiredService<McpClient>();
                var skillsProvider = mcpClient.CreateSkillsProvider(
                    new HashSet<string>(["store-policies"], StringComparer.OrdinalIgnoreCase),
                    sp.GetService<ILoggerFactory>()
                );

                var (options, _) = AgentDefinitionLoader.Load(DefinitionFile, key);
                options.AIContextProviders = [skillsProvider];

                var agent = new ChatClientAgent(chatClient, options);

                return agent.WithBookWormGovernance(sp, key);
            });
        }
    }
}
