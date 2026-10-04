using Mediator;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Features.Dashboard;

public sealed record GetCatalogDashboardQuery : IQuery<CatalogDashboardDto>;

internal sealed class GetCatalogDashboardHandler(IBookRepository repository, IFusionCache cache)
    : IQueryHandler<GetCatalogDashboardQuery, CatalogDashboardDto>
{
    public async ValueTask<CatalogDashboardDto> Handle(
        GetCatalogDashboardQuery request,
        CancellationToken cancellationToken
    )
    {
        return await cache.GetOrSetAsync(
            "catalog:dashboard:v1",
            async ct =>
            {
                var categories = await repository.CountByCategoryAsync(ct);
                return new CatalogDashboardDto(
                    categories.Sum(category => category.Count),
                    [
                        .. categories.Select(category => new CategoryCountDto(
                            category.Name,
                            category.Count
                        )),
                    ]
                );
            },
            new FusionCacheEntryOptions { Duration = TimeSpan.FromSeconds(15) },
            cancellationToken
        );
    }
}
