using BookWorm.Shared.Constants;
using FluentValidation;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(DataSchemaLength.SuperLarge);
    }
}
