namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookValidator(IValidator<IFormFile> validator)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.Description).MaximumLength(DataSchemaLength.SuperLarge);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.PriceSale).GreaterThan(0).LessThanOrEqualTo(x => x.Price);

        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.PublisherId).NotEmpty();

        RuleFor(x => x.AuthorIds).NotEmpty();

        When(IsHasFiles, () => RuleFor(x => x.Image!).SetValidator(validator));
    }

    private static bool IsHasFiles(CreateBookCommand command)
    {
        return command.Image is not null;
    }
}
