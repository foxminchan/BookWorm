using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public partial class ProductDescription
{
    [Parameter]
    public string? Description { get; set; }
}
