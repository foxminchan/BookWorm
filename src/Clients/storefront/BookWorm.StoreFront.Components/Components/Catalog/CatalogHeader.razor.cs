using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public sealed partial class CatalogHeader
{
    [Parameter]
    public int FilteredCount { get; set; }

    [Parameter]
    public int TotalCount { get; set; }

    [Parameter]
    public string CurrentSortBy { get; set; } = "price-low";

    [Parameter]
    public Action<string>? CurrentSortByChanged { get; set; }

    [Parameter]
    public Action? OnSortChanged { get; set; }

    private readonly List<SortOption> _sortOptions =
    [
        new("price-low", "Price: Low to High"),
        new("price-high", "Price: High to Low"),
        new("name-asc", "Name: A to Z"),
        new("name-desc", "Name: Z to A"),
        new("rating", "Rating"),
        new("reviews", "Most Reviews"),
    ];

    private sealed record SortOption(string Value, string Label);
}
