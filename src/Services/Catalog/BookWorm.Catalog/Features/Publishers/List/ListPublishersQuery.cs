using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Catalog.Features.Publishers.List;

public sealed record ListPublishersQuery : IQuery<IReadOnlyList<PublisherDto>>;

internal sealed class ListPublishersHandler(IPublisherRepository repository, IFusionCache cache)
    : IQueryHandler<ListPublishersQuery, IReadOnlyList<PublisherDto>>
{
    public async ValueTask<IReadOnlyList<PublisherDto>> Handle(
        ListPublishersQuery request,
        CancellationToken cancellationToken
    )
    {
        return await cache.GetOrSetAsync(
            CatalogCacheKeys.Publishers,
            async ct =>
            {
                var publishers = await repository.ListAsync(ct);

                return publishers.ToPublisherDtos();
            },
            token: cancellationToken
        );
    }
}
