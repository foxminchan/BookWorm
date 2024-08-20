namespace BookWorm.Ordering.Features.Orders.Create;

internal sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(DataSchemaLength.SuperLarge);
    }
}
