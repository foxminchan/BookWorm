using BookWorm.Shared.Constants;
using FluentValidation;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookValidator()
    {
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
