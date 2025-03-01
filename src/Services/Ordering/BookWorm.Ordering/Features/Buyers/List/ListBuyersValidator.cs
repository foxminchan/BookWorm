namespace BookWorm.Ordering.Features.Buyers.List;

public sealed class ListBuyersValidator : AbstractValidator<ListBuyersQuery>
{
    public ListBuyersValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0);

        RuleFor(x => x.PageSize).GreaterThan(0);
    }
}
