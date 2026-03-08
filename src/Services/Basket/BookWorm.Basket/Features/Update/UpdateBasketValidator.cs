namespace BookWorm.Basket.Features.Update;

internal sealed class UpdateBasketValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketValidator()
    {
        RuleFor(x => x.Items).ApplyItemRules();
    }
}
