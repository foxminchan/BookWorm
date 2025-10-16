using System.ComponentModel;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.SharedKernel;
using Mediator;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Features.Visualize;

public sealed record VisualizeWorkflowQuery(
    [property: Description("The type of visualization to generate")]
    [property: DefaultValue(Visualizations.Mermaid)]
        Visualizations Type = Visualizations.Mermaid
) : IQuery<string>;

public sealed class VisualizerWorkflowHandler(IAgentOrchestrationService agentOrchestrationService)
    : IQueryHandler<VisualizeWorkflowQuery, string>
{
    public ValueTask<string> Handle(
        VisualizeWorkflowQuery request,
        CancellationToken cancellationToken
    )
    {
        var workflow = agentOrchestrationService.BuildAgentsWorkflow();

        return ValueTask.FromResult(
            request.Type switch
            {
                Visualizations.Mermaid => workflow.ToMermaidString(),
                Visualizations.Dot => workflow.ToDotString(),
                _ => string.Empty,
            }
        );
    }
}
