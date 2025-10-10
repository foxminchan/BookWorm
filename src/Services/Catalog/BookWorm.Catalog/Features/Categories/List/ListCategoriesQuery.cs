using Mediator;

namespace BookWorm.Catalog.Features.Categories.List;

public sealed record ListCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;

public sealed class ListCategoriesHandler(ICategoryRepository repository)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async ValueTask<IReadOnlyList<CategoryDto>> Handle(
        ListCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var categories = await repository.ListAsync(cancellationToken);

        return categories.ToCategoryDtos();
    }
}
