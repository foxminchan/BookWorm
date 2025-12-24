using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Filter;

public sealed partial class PriceRangeFilter
{
    private const decimal DefaultMinPrice = 0;
    private const decimal DefaultMaxPrice = 9999;
    private const decimal PriceStep = 10;
    private const decimal RangeBoundary1 = 50;
    private const decimal RangeBoundary2 = 100;
    private const decimal RangeBoundary3 = 200;

    private decimal? _localMinPrice;
    private decimal? _localMaxPrice;

    [Parameter]
    public decimal? MinPrice { get; set; }

    [Parameter]
    public decimal? MaxPrice { get; set; }

    [Parameter]
    public Action<decimal?>? OnMinPriceChanged { get; set; }

    [Parameter]
    public Action<decimal?>? OnMaxPriceChanged { get; set; }

    [Parameter]
    public Action<(decimal? Min, decimal? Max)>? OnPriceRangeSelected { get; set; }

    protected override void OnParametersSet()
    {
        _localMinPrice = MinPrice;
        _localMaxPrice = MaxPrice;
    }

    private void HandleMinPriceChange(decimal value)
    {
        OnMinPriceChanged?.Invoke(value == DefaultMinPrice ? null : value);
    }

    private void HandleMaxPriceChange(decimal value)
    {
        OnMaxPriceChanged?.Invoke(value == DefaultMaxPrice ? null : value);
    }

    private void HandlePriceRangeClick(decimal? min, decimal? max)
    {
        OnPriceRangeSelected?.Invoke((min, max));
    }
}
