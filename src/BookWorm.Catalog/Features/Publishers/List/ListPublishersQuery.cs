using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Publishers.List;

public sealed record ListPublishersQuery : IQuery<Result<IEnumerable<Publisher>>>;

public sealed class ListPublishersHandler(IReadRepository<Publisher> repository)
    : IQueryHandler<ListPublishersQuery, Result<IEnumerable<Publisher>>>
{
    public async Task<Result<IEnumerable<Publisher>>> Handle(
        ListPublishersQuery request,
        CancellationToken cancellationToken
    )
    {
        var publishers = await repository.ListAsync(cancellationToken);

        return publishers;
    }
}
