using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Agents.CustomerSupport;
using BookWorm.Chat.Agents.LanguageTranslation;
using BookWorm.Chat.Agents.Routing;
using BookWorm.Chat.Agents.SentimentAnalysis;
using BookWorm.Chat.Agents.Summarization;
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

            Span<string> agentNames =
            [
                QAAgentDefinition.Name,
                RouterAgentDefinition.Name,
                LanguageAgentDefinition.Name,
                SummarizeAgentDefinition.Name,
                SentimentAgentDefinition.Name,
            ];

            // Map A2A endpoints
            foreach (string agentName in agentNames)
            {
                app.MapA2AJsonRpc(agentName, $"/a2a/{agentName}").WithTags(agentName);

                app.MapA2AHttpJson(agentName, $"/a2a/{agentName}").WithTags(agentName);
            }

            // Map AG-UI endpoint for interactive agents (e.g. RouterAgent)
            app.MapAGUI("/ag-ui", Workflows.Chat)
                .WithSummary("Interactive AI Agent")
                .WithTags(nameof(Chat));

            // Map OpenAI Chat Completions
            Span<string> chatCompletionAgentNames =
            [
                RouterAgentDefinition.Name,
                LanguageAgentDefinition.Name,
                SummarizeAgentDefinition.Name,
                SentimentAgentDefinition.Name,
            ];

            foreach (string agentName in chatCompletionAgentNames)
            {
                app.MapOpenAIChatCompletions(
                        app.Services.GetRequiredKeyedService<AIAgent>(agentName)
                    )
                    .WithTags(agentName);
            }
        }
    }
}
