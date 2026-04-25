namespace BookWorm.Basket.Features;

internal static class BasketItemRules
{
    extension<T>(IRuleBuilder<T, List<BasketItemRequest>> ruleBuilder)
    {
        public void ApplyItemRules()
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
}
