using BookWorm.StoreFront.Components.Models;
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
    [EditorRequired]
    public required List<Publisher> AvailablePublishers { get; set; }

    [Parameter]
    public EventCallback<decimal?> OnMinPriceChanged { get; set; }

    [Parameter]
    public EventCallback<decimal?> OnMaxPriceChanged { get; set; }

    [Parameter]
    public EventCallback<(decimal? Min, decimal? Max)> OnPriceRangeSelected { get; set; }

    [Parameter]
    public EventCallback<(Guid PublisherId, bool IsChecked)> OnPublisherToggled { get; set; }

    [Parameter]
    public EventCallback OnClearFilters { get; set; }

    private async Task HandleClearFilters()
    {
        await OnClearFilters.InvokeAsync();
    }
}
