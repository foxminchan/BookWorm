namespace BookWorm.Basket.Features.Update;

public sealed class UpdateBasketValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketValidator()
    {
        RuleFor(x => x.Items).ApplyItemRules();
    }
}
