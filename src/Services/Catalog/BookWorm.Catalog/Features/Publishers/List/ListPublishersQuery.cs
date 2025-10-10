using Mediator;

namespace BookWorm.Catalog.Features.Publishers.List;

public sealed record ListPublishersQuery : IQuery<IReadOnlyList<PublisherDto>>;

public sealed class ListPublishersHandler(IPublisherRepository repository)
    : IQueryHandler<ListPublishersQuery, IReadOnlyList<PublisherDto>>
{
    public async ValueTask<IReadOnlyList<PublisherDto>> Handle(
        ListPublishersQuery request,
        CancellationToken cancellationToken
    )
    {
        var publishers = await repository.ListAsync(cancellationToken);

        return publishers.ToPublisherDtos();
    }
}
