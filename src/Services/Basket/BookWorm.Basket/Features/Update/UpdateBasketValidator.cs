namespace BookWorm.Basket.Features.Update;

public sealed class UpdateBasketValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketValidator()
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
