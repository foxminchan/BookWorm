namespace BookWorm.Basket.Features.Create;

public sealed class CreateBasketValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketValidator()
    {
        RuleFor(x => x.Items).ApplyItemRules();
    }
}
