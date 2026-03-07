namespace BookWorm.Basket.Features;

internal static class BasketItemRules
{
    public static void ApplyItemRules<T>(this IRuleBuilder<T, List<BasketItemRequest>> ruleBuilder)
    {
        ruleBuilder
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
