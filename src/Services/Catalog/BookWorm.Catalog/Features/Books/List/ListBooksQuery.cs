using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Infrastructure.GenAi.Search;

namespace BookWorm.Catalog.Features.Books.List;

public sealed record ListBooksQuery(
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageIndex)]
        int PageIndex = Pagination.DefaultPageIndex,
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageSize)]
        int PageSize = Pagination.DefaultPageSize,
    [property: Description("Property to order results by")]
    [property: DefaultValue(nameof(Book.Name))]
        string? OrderBy = nameof(Book.Name),
    [property: Description("Whether to order results in descending order")]
    [property: DefaultValue(false)]
        bool IsDescending = false,
    [property: Description("Search term to filter results by")]
    [property: DefaultValue(null)]
        string? Search = null,
    [property: Description("Minimum price to filter results by")]
    [property: DefaultValue(null)]
        decimal? MinPrice = null,
    [property: Description("Maximum price to filter results by")]
    [property: DefaultValue(null)]
        decimal? MaxPrice = null,
    [property: Description("Category IDs to filter results by")]
    [property: DefaultValue(null)]
        Guid[]? CategoryId = null,
    [property: Description("Publisher IDs to filter results by")]
    [property: DefaultValue(null)]
        Guid[]? PublisherId = null,
    [property: Description("Author IDs to filter results by")]
    [property: DefaultValue(null)]
        Guid[]? AuthorIds = null
) : IQuery<PagedResult<BookDto>>;

public sealed class ListBooksHandler(
    ISearch search,
    IBookRepository repository,
    IMapper<Book, BookDto> mapper
) : IQueryHandler<ListBooksQuery, PagedResult<BookDto>>
{
    public async Task<PagedResult<BookDto>> Handle(
        ListBooksQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid[] ids = [];
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var response = await search.SearchAsync(
                request.Search,
                [nameof(Book), request.Search],
                nameof(Book).ToLower(),
                cancellationToken: cancellationToken
            );

            ids = response.Select(x => x.Id).ToArray();
        }

        var filterSpec = new BookFilterSpec(
            request.PageIndex,
            request.PageSize,
            request.OrderBy,
            request.IsDescending,
            request.MinPrice,
            request.MaxPrice,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds,
            ids
        );

        var books = await repository.ListAsync(filterSpec, cancellationToken);

        var countSpec = new BookFilterSpec(
            request.MinPrice,
            request.MaxPrice,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds,
            ids
        );

        var totalItems = await repository.CountAsync(countSpec, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var results = mapper.MapToDtos(books);

        return new(results, request.PageIndex, request.PageSize, totalItems, totalPages);
    }
}
