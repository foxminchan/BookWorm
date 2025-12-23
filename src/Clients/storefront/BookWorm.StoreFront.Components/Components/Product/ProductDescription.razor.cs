using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Product;

public partial class ProductDescription
{
    [Parameter]
    public string? Description { get; set; }
}
