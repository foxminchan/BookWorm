using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public sealed partial class CatalogFilters
{
    [Parameter]
    public decimal? MinPrice { get; set; }

    [Parameter]
    public decimal? MaxPrice { get; set; }

    [Parameter]
    public HashSet<Guid> SelectedPublisherIds { get; set; } = [];

    [Parameter]
    public Action<decimal?>? OnMinPriceChanged { get; set; }

    [Parameter]
    public Action<decimal?>? OnMaxPriceChanged { get; set; }

    [Parameter]
    public Action<(decimal? Min, decimal? Max)>? OnPriceRangeSelected { get; set; }

    [Parameter]
    public Action<(Guid PublisherId, bool IsChecked)>? OnPublisherToggled { get; set; }

    [Parameter]
    public Action? OnClearFilters { get; set; }

    private void HandleClearFilters()
    {
        OnClearFilters?.Invoke();
    }
}
