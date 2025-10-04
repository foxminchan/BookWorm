using BookWorm.Chassis.CQRS.Query;
using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.SharedKernel;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Features.Visualize;

public sealed record VisualizeWorkflowQuery(
    [property: Description("The type of visualization to generate")]
    [property: DefaultValue(VisualizationType.Mermaid)]
        VisualizationType Type = VisualizationType.Mermaid
) : IQuery<string>;

public sealed class VisualizerWorkflowHandler(ISummarizer summarizer)
    : IQueryHandler<VisualizeWorkflowQuery, string>
{
    public Task<string> Handle(VisualizeWorkflowQuery request, CancellationToken cancellationToken)
    {
        var workflow = summarizer.BuildAgentsWorkflow();

        return Task.FromResult(
            request.Type switch
            {
                VisualizationType.Mermaid => workflow.ToMermaidString(),
                VisualizationType.Dot => workflow.ToDotString(),
                _ => string.Empty,
            }
        );
    }
}
