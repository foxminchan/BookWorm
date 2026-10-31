using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.CustomerSupport;

internal static class QAAgentRegistration
{
    private const string DefinitionFile = "Agents/CustomerSupport/qa-agent.yaml";

    extension(IHostApplicationBuilder builder)
    {
        public void AddQAAgent()
        {
            builder
                .AddAIAgent(
                    Constants.Other.Agents.QAAgent,
                    (sp, key) =>
                    {
                        var compactionProvider = CompactionPipelineFactory.CreateLight();

                        var chatClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UsePIIMiddleware(sp)
                            .UseGuardrailMiddleware()
                            .UseAIContextProviders(compactionProvider)
                            .Build(sp);

                        var skillsProvider = new AgentSkillsProvider(
                            Path.Combine(AppContext.BaseDirectory, "Skills", "store-policies"),
                            loggerFactory: sp.GetService<ILoggerFactory>()
                        );

                        var (options, _) = AgentDefinitionLoader.Load(DefinitionFile, key);
                        options.AIContextProviders = [skillsProvider];

                        var agent = new ChatClientAgent(chatClient, options);

                        return agent.WithBookWormGovernance(sp, key);
                    }
                )
                .AddA2AServer();
        }
    }
}
