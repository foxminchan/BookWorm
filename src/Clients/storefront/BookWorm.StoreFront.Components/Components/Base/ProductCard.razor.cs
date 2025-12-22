using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Base;

public sealed partial class ProductCard
{
    [Parameter]
    [EditorRequired]
    public required Book Book { get; set; }

    [Parameter]
    public bool ShowAddToCart { get; set; } = true;

    [Parameter]
    public EventCallback<Guid> OnAddToCart { get; set; }

    private async Task HandleAddToCart()
    {
        await OnAddToCart.InvokeAsync(Book.Id);
    }
}
