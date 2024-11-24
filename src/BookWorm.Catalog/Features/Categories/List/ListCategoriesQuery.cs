using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Categories.List;

public sealed record ListCategoriesQuery : IQuery<Result<IEnumerable<Category>>>;

public sealed class ListCategoriesHandler(IReadRepository<Category> repository)
    : IQueryHandler<ListCategoriesQuery, Result<IEnumerable<Category>>>
{
    public async Task<Result<IEnumerable<Category>>> Handle(
        ListCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var categories = await repository.ListAsync(cancellationToken);

        return categories;
    }
}
