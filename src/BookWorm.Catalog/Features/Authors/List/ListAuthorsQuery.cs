using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Authors.List;

public sealed record ListAuthorsQuery : IQuery<Result<IEnumerable<Author>>>;

public sealed class ListAuthorsHandler(IReadRepository<Author> repository)
    : IQueryHandler<ListAuthorsQuery, Result<IEnumerable<Author>>>
{
    public async Task<Result<IEnumerable<Author>>> Handle(
        ListAuthorsQuery request,
        CancellationToken cancellationToken
    )
    {
        var authors = await repository.ListAsync(cancellationToken);

        return authors;
    }
}
