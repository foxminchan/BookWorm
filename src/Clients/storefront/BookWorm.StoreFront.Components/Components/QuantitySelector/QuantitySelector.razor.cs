using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.QuantitySelector;

public partial class QuantitySelector
{
    [Parameter]
    public int Quantity { get; set; } = 1;

    [Parameter]
    public EventCallback<int> QuantityChanged { get; set; }

    [Parameter]
    public bool IsOutOfStock { get; set; }

    [Parameter]
    public EventCallback<int> OnAddToCartClick { get; set; }

    private async Task IncrementQuantity()
    {
        if (Quantity < 99)
        {
            Quantity++;
            await QuantityChanged.InvokeAsync(Quantity);
        }
    }

    private async Task DecrementQuantity()
    {
        if (Quantity > 1)
        {
            Quantity--;
            await QuantityChanged.InvokeAsync(Quantity);
        }
    }

    private async Task OnAddToCart()
    {
        await OnAddToCartClick.InvokeAsync(Quantity);
    }
}
