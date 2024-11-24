namespace BookWorm.Catalog.Features.Books.List;

internal sealed class ListBooksValidator : AbstractValidator<ListBooksQuery>
{
    public ListBooksValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0);

        RuleFor(x => x.PageSize).GreaterThan(0);

        RuleFor(x => x.Statuses).ForEach(x => x.IsInEnum());
    }
}
