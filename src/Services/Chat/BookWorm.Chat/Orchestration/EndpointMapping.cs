using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Features.CustomerSupport;
using BookWorm.Chat.Features.LanguageTranslation;
using BookWorm.Chat.Features.Routing;
using BookWorm.Chat.Features.SentimentAnalysis;
using BookWorm.Chat.Features.Summarization;
using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace BookWorm.Chat.Orchestration;

internal static class EndpointMapping
{
    public static void MapAgentsDiscovery(this WebApplication app)
    {
        app.MapAgentDiscovery("/agents");

        // Map A2A
        app.MapA2A(
                QAAgentDefinition.Name,
                $"/a2a/{QAAgentDefinition.Name}",
                QAAgentDefinition.AgentCard
            )
            .WithTags(QAAgentDefinition.Name);
        app.MapA2A(
                RouterAgentDefinition.Name,
                $"/a2a/{RouterAgentDefinition.Name}",
                RouterAgentDefinition.AgentCard
            )
            .WithTags(RouterAgentDefinition.Name);
        app.MapA2A(
                LanguageAgentDefinition.Name,
                $"/a2a/{LanguageAgentDefinition.Name}",
                LanguageAgentDefinition.AgentCard
            )
            .WithTags(LanguageAgentDefinition.Name);
        app.MapA2A(
                SummarizeAgentDefinition.Name,
                $"/a2a/{SummarizeAgentDefinition.Name}",
                SummarizeAgentDefinition.AgentCard
            )
            .WithTags(SummarizeAgentDefinition.Name);
        app.MapA2A(
                SentimentAgentDefinition.Name,
                $"/a2a/{SentimentAgentDefinition.Name}",
                SentimentAgentDefinition.AgentCard
            )
            .WithTags(SentimentAgentDefinition.Name);

        // Map AG-UI
        app.MapAGUI("/ag-ui", app.Services.GetRequiredKeyedService<AIAgent>(Workflows.Chat))
            .WithSummary("Interactive AI Agent")
            .WithTags(nameof(Chat));

        // Map OpenAI Chat Completions
        app.MapOpenAIChatCompletions(
                app.Services.GetRequiredKeyedService<AIAgent>(SummarizeAgentDefinition.Name)
            )
            .WithTags(SummarizeAgentDefinition.Name);
        app.MapOpenAIChatCompletions(
                app.Services.GetRequiredKeyedService<AIAgent>(LanguageAgentDefinition.Name)
            )
            .WithTags(LanguageAgentDefinition.Name);
        app.MapOpenAIChatCompletions(
                app.Services.GetRequiredKeyedService<AIAgent>(SentimentAgentDefinition.Name)
            )
            .WithTags(SentimentAgentDefinition.Name);
    }
}
