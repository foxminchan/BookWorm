namespace BookWorm.Chat.Features.Visualizer;

public sealed class VisualizerWorkValidator : AbstractValidator<VisualizerWorkflowQuery>
{
    public VisualizerWorkValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
    }
}
