using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Features.Categories.List;

public sealed record ListCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;

internal sealed class ListCategoriesHandler(ICategoryRepository repository, IFusionCache cache)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async ValueTask<IReadOnlyList<CategoryDto>> Handle(
        ListCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        return await cache.GetOrSetAsync(
            CatalogCacheKeys.Categories,
            async ct =>
            {
                var categories = await repository.ListAsync(ct);

                return categories.ToCategoryDtos();
            },
            token: cancellationToken
        );
    }
}
