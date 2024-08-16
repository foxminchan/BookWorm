using BookWorm.Shared.Constants;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(DataSchemaLength.SuperLarge);
    }
}
