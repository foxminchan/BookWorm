using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Basket;

public sealed partial class OrderSummary
{
    private List<TrustBadge> _trustBadges = [];

    [Parameter]
    [EditorRequired]
    public required int TotalItems { get; set; }

    [Parameter]
    [EditorRequired]
    public required decimal Subtotal { get; set; }

    [Parameter]
    [EditorRequired]
    public required decimal Total { get; set; }

    [Parameter]
    public decimal TotalSavings { get; set; }

    [Parameter]
    public EventCallback OnCheckout { get; set; }

    protected override void OnInitialized()
    {
        _trustBadges =
        [
            new("/icons/check-circle.svg", "Cash on Delivery", "Cash on Delivery"),
            new("/icons/check.svg", "Free returns", "Free returns within 30 days"),
            new("/icons/package.svg", "Fast shipping", "Fast & reliable shipping"),
        ];
    }

    private record TrustBadge(string IconUrl, string AltText, string Text);
}
