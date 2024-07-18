using Ardalis.Result;
using BookWorm.Catalog.Domain;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Categories.List;

public sealed record ListCategoriesQuery : IQuery<Result<IEnumerable<Category>>>;

public sealed class ListCategoriesQueryHandler(IRepository<Category> repository)
    : IQueryHandler<ListCategoriesQuery, Result<IEnumerable<Category>>>
{
    public async Task<Result<IEnumerable<Category>>> Handle(ListCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(cancellationToken);

        return categories;
    }
}
