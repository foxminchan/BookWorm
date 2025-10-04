namespace BookWorm.Chat.Features.Visualizer;

public sealed class VisualizeWorkflowValidator : AbstractValidator<VisualizeWorkflowQuery>
{
    public VisualizeWorkflowValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
    }
}
