using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Product;

public partial class ProductDetails
{
    [Parameter]
    public Publisher? Publisher { get; set; }

    [Parameter]
    public Category? Category { get; set; }
}
