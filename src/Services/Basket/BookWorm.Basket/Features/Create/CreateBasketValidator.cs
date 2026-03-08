namespace BookWorm.Basket.Features.Create;

internal sealed class CreateBasketValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketValidator()
    {
        RuleFor(x => x.Items).ApplyItemRules();
    }
}
