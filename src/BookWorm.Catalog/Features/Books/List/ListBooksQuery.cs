using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Books.List;

public sealed record ListBooksQuery : IQuery<PagedResult<IEnumerable<Book>>>
{
    public int PageIndex { get; init; } = Pagination.DefaultPageIndex;
    public int PageSize { get; init; } = Pagination.DefaultPageSize;
    public string? OrderBy { get; init; }
    public bool IsDescending { get; init; }
    public Status[]? Statuses { get; init; }
    public Guid[]? CategoryId { get; init; }
    public Guid[]? PublisherId { get; init; }
    public Guid[]? AuthorIds { get; init; }
    public string? Search { get; init; }
}

public sealed class ListBooksHandler(IAiService aiService, IReadRepository<Book> repository)
    : IQueryHandler<ListBooksQuery, PagedResult<IEnumerable<Book>>>
{
    public async Task<PagedResult<IEnumerable<Book>>> Handle(
        ListBooksQuery request,
        CancellationToken cancellationToken
    )
    {
        Vector? vector = null;
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            vector = await aiService.GetEmbeddingAsync(request.Search, cancellationToken);
        }

        BookFilterSpec spec = new(
            request.PageIndex,
            request.PageSize,
            request.OrderBy,
            request.IsDescending,
            request.Statuses,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds,
            vector
        );

        var books = await repository.ListAsync(spec, cancellationToken);

        var totalRecords = await repository.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        PagedInfo pagedInfo = new(request.PageIndex, request.PageSize, totalPages, totalRecords);

        return new(pagedInfo, books);
    }
}
