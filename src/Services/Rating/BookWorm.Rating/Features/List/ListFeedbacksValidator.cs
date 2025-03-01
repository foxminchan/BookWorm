namespace BookWorm.Rating.Features.List;

public sealed class ListFeedbacksValidator : AbstractValidator<ListFeedbacksQuery>
{
    public ListFeedbacksValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0);

        RuleFor(x => x.PageSize).GreaterThan(0);
    }
}
