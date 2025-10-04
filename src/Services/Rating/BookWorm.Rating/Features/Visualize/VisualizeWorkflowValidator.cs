namespace BookWorm.Rating.Features.Visualize;

public sealed class VisualizeWorkflowValidator : AbstractValidator<VisualizeWorkflowQuery>
{
    public VisualizeWorkflowValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
    }
}
