namespace BookWorm.StoreFront.Components.Components.Custom.Home;

public sealed partial class BottomFeatures
{
    private sealed record Feature(
        string? IconPath,
        string Title,
        string Description,
        string? Emoji = null
    );

    private List<Feature> _features = [];

    protected override void OnInitialized()
    {
        _features =
        [
            new("/icons/shipping.svg", "Free Shipping", "On all orders over $75"),
            new("/icons/refresh.svg", "Easy Returns", "30-day return policy"),
            new("/icons/shield.svg", "Secure Checkout", "SSL & protected shopping"),
            new(null, "Customer Support", "Available 24/7 for you", "💬"),
        ];
    }
}
