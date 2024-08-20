namespace BookWorm.Catalog.Features.Books.Update;

internal sealed class UpdateBookValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.Description)
            .MaximumLength(DataSchemaLength.SuperLarge);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.PriceSale)
            .GreaterThan(0)
            .LessThanOrEqualTo(x => x.Price);

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
