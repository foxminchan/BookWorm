namespace BookWorm.Catalog.Features.Books;

internal interface IBookInput
{
    string Name { get; }
    string Description { get; }
    IFormFile? Image { get; }
    decimal Price { get; }
    decimal? PriceSale { get; }
    Guid CategoryId { get; }
    Guid PublisherId { get; }
    Guid[] AuthorIds { get; }
}

internal abstract class BookValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IBookInput
{
    protected BookValidatorBase()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.Description).MaximumLength(DataSchemaLength.SuperLarge);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.PriceSale).GreaterThan(0).LessThanOrEqualTo(x => x.Price);

        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.PublisherId).NotEmpty();

        RuleFor(x => x.AuthorIds).NotEmpty();

        When(x => x.Image is not null, () => RuleFor(x => x.Image).ApplyImageRules());
    }
}
