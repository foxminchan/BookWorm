namespace BookWorm.Basket.Features.Create;

public sealed class CreateBasketValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .ForEach(itemRule =>
                itemRule.ChildRules(basketItem =>
                {
                    basketItem.RuleFor(a => a.Id).NotEmpty();
                    basketItem.RuleFor(a => a.Quantity).GreaterThan(0);
                })
            );
    }
}
