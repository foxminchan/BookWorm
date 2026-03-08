namespace BookWorm.Ordering.Features.Orders.List;

internal sealed class ListOrdersValidator : AbstractValidator<ListOrdersQuery>
{
    public ListOrdersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0);

        RuleFor(x => x.PageSize).GreaterThan(0);

        RuleFor(x => x.Status).IsInEnum();
    }
}
