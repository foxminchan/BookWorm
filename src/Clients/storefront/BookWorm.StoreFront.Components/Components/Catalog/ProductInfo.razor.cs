using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public partial class ProductInfo
{
    [Parameter, EditorRequired]
    public required string Name { get; set; }

    [Parameter]
    public IReadOnlyList<Author> Authors { get; set; } = [];

    [Parameter]
    public double AverageRating { get; set; }

    [Parameter]
    public int TotalReviews { get; set; }

    [Parameter]
    public decimal Price { get; set; }

    [Parameter]
    public decimal? PriceSale { get; set; }
}
