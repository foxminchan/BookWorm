using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public sealed partial class PriceRangeFilter : ComponentBase
{
    private const decimal DefaultMinPrice = 0;
    private const decimal DefaultMaxPrice = 9999;
    private const decimal PriceStep = 10;
    private const decimal RangeBoundary1 = 50;
    private const decimal RangeBoundary2 = 100;
    private const decimal RangeBoundary3 = 200;

    [Parameter]
    public decimal? MinPrice { get; set; }

    [Parameter]
    public decimal? MaxPrice { get; set; }

    [Parameter]
    public EventCallback<decimal?> OnMinPriceChanged { get; set; }

    [Parameter]
    public EventCallback<decimal?> OnMaxPriceChanged { get; set; }

    [Parameter]
    public EventCallback<(decimal? Min, decimal? Max)> OnPriceRangeSelected { get; set; }

    private async Task HandleMinPriceChange(decimal value)
    {
        await OnMinPriceChanged.InvokeAsync(value == DefaultMinPrice ? null : value);
    }

    private async Task HandleMaxPriceChange(decimal value)
    {
        await OnMaxPriceChanged.InvokeAsync(value == DefaultMaxPrice ? null : value);
    }

    private async Task HandlePriceRangeClick(decimal? min, decimal? max)
    {
        await OnPriceRangeSelected.InvokeAsync((min, max));
    }
}
