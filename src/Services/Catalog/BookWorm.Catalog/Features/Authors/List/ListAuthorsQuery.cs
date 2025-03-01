namespace BookWorm.Catalog.Features.Authors.List;

public sealed record ListAuthorsQuery : IQuery<IReadOnlyList<AuthorDto>>;

public sealed class ListAuthorsHandler(IAuthorRepository repository)
    : IQueryHandler<ListAuthorsQuery, IReadOnlyList<AuthorDto>>
{
    public async Task<IReadOnlyList<AuthorDto>> Handle(
        ListAuthorsQuery request,
        CancellationToken cancellationToken
    )
    {
        var authors = await repository.ListAsync(cancellationToken);

        return authors.ToAuthorDtos();
    }
}
