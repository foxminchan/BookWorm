using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Features.Authors.List;

public sealed record ListAuthorsQuery : IQuery<IReadOnlyList<AuthorDto>>;

internal sealed class ListAuthorsHandler(IAuthorRepository repository, IFusionCache cache)
    : IQueryHandler<ListAuthorsQuery, IReadOnlyList<AuthorDto>>
{
    public async ValueTask<IReadOnlyList<AuthorDto>> Handle(
        ListAuthorsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await cache.GetOrSetAsync(
            CatalogCacheKeys.Authors,
            async ct =>
            {
                var authors = await repository.ListAsync(ct);

                return authors.ToAuthorDtos();
            },
            token: cancellationToken
        );
    }
}
