using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Agents.CustomerSupport;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace BookWorm.Chat.Orchestration;

internal static class EndpointMapping
{
    extension(WebApplication app)
    {
        public void MapAgentsDiscovery()
        {
            app.MapAgentDiscovery("/agents");

            // Dynamically discover all agents that have an A2AServer registered via
            // A2AServerServiceCollectionExtensions.AddA2AServer(), which keys each
            // A2AServer instance by agent name in the DI container.
            // Agents are resolved once here and reused for both A2A and chat completion mapping.
            ReadOnlySpan<AIAgent> agents =
            [
                .. app
                    .Services.GetKeyedServices<AIAgent>(KeyedService.AnyKey)
                    .Where(agent =>
                        agent.Name is not null
                        && app.Services.GetKeyedService<A2AServer>(agent.Name) is not null
                    ),
            ];

            // Map A2A endpoints and OpenAI Chat Completions in a single pass
            foreach (var agent in agents)
            {
                var agentName = agent.Name ?? string.Empty;

                app.MapA2AJsonRpc(agent, $"/a2a/{agentName}").WithTags(agentName);

                app.MapA2AHttpJson(agent, $"/a2a/{agentName}").WithTags(agentName);

                // QAAgent is handoff-only and not directly callable via chat completions
                if (
                    string.Compare(agentName, QAAgentDefinition.Name, StringComparison.Ordinal) != 0
                )
                {
                    app.MapOpenAIChatCompletions(agent).WithTags(agentName);
                }
            }

            // Map AG-UI endpoint for interactive agents (e.g. RouterAgent)
            app.MapAGUI(Workflows.Chat, "/ag-ui")
                .WithSummary("Interactive AI Agent")
                .WithTags(nameof(Chat));

            app.MapOpenAIResponses();

            app.MapOpenAIConversations();
        }
    }
}
