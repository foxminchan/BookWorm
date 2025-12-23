using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Product;

public partial class ProductImage
{
    [Parameter, EditorRequired]
    public required string ImageUrl { get; set; }

    [Parameter, EditorRequired]
    public required string AltText { get; set; }

    [Parameter]
    public decimal? PriceSale { get; set; }
}
