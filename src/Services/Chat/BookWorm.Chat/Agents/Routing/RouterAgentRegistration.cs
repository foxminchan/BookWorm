using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.Routing;

internal static class RouterAgentRegistration
{
    private const string DefinitionFile = "Agents/Routing/router-agent.yaml";

    extension(IHostApplicationBuilder builder)
    {
        public void AddRouterAgent()
        {
            builder
                .AddAIAgent(
                    Constants.Other.Agents.RouterAgent,
                    (sp, key) =>
                    {
                        var chatClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UseGuardrailMiddleware()
                            .Build(sp);

                        var (options, _) = AgentDefinitionLoader.Load(DefinitionFile, key);
                        options.ChatOptions!.Reasoning = new() { Effort = ReasoningEffort.Low };

                        var agent = new ChatClientAgent(chatClient, options);

                        return agent.WithBookWormGovernance(sp, key);
                    }
                )
                .AddA2AServer();
        }
    }
}
