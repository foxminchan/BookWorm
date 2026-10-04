namespace BookWorm.Catalog.Features.Dashboard;

public sealed record CatalogDashboardDto(
    long TotalBooks,
    IReadOnlyList<CategoryCountDto> Categories
);

public sealed record CategoryCountDto(string Name, long Value);
