using BookWorm.Constants.Other;
using BookWorm.SharedKernel;
using Mediator;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Features.Visualize;

[ExcludeFromCodeCoverage]
public sealed record VisualizeWorkflowQuery(
    [property: Description("The type of visualization to generate")]
    [property: DefaultValue(Visualizations.Mermaid)]
        Visualizations Type = Visualizations.Mermaid
) : IQuery<string>;

[ExcludeFromCodeCoverage]
public sealed class VisualizerWorkflowHandler(
    [FromKeyedServices(Workflows.RatingSummarizer)] Workflow summarizer
) : IQueryHandler<VisualizeWorkflowQuery, string>
{
    public ValueTask<string> Handle(
        VisualizeWorkflowQuery request,
        CancellationToken cancellationToken
    )
    {
        return ValueTask.FromResult(
            request.Type switch
            {
                Visualizations.Mermaid => summarizer.ToMermaidString(),
                Visualizations.Dot => summarizer.ToDotString(),
                _ => string.Empty,
            }
        );
    }
}
