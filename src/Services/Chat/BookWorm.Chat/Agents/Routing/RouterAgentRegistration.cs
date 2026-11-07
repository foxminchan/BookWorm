namespace BookWorm.Chat.Agents.Routing;

internal static class RouterAgentRegistration
{
    private const string DefinitionFile = "Agents/Routing/router-agent.yaml";

    extension(IHostApplicationBuilder builder)
    {
        public void AddRouterAgent()
        {
            builder.AddAIAgent(
                Constants.Other.Agents.RouterAgent,
                (sp, key) =>
                {
                    var compactionProvider = CompactionPipelineFactory.CreateLight();

                    var chatClient = sp.GetRequiredService<IChatClient>()
                        .AsBuilder()
                        .UsePIIMiddleware(sp)
                        .UseGuardrailMiddleware()
                        .UseAIContextProviders(compactionProvider)
                        .Build(sp);

                    var (options, _) = AgentDefinitionLoader.Load(DefinitionFile, key);

                    var agent = new ChatClientAgent(chatClient, options);

                    return agent.WithBookWormGovernance(sp, key);
                }
            );
        }
    }
}
