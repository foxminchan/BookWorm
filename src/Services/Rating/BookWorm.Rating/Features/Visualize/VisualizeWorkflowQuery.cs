using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.SharedKernel;
using Mediator;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Features.Visualize;

public sealed record VisualizeWorkflowQuery(
    [property: Description("The type of visualization to generate")]
    [property: DefaultValue(Visualizations.Mermaid)]
        Visualizations Type = Visualizations.Mermaid
) : IQuery<string>;

public sealed class VisualizerWorkflowHandler(ISummarizer summarizer)
    : IQueryHandler<VisualizeWorkflowQuery, string>
{
    public ValueTask<string> Handle(
        VisualizeWorkflowQuery request,
        CancellationToken cancellationToken
    )
    {
        var workflow = summarizer.BuildAgentsWorkflow();

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
