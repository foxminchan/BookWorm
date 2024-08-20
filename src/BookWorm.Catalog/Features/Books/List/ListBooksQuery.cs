using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Books.List;

public sealed record ListBooksQuery(
    int PageIndex,
    int PageSize,
    string? OrderBy,
    bool IsDescending,
    Status[]? Statuses,
    Guid[]? CategoryId,
    Guid[]? PublisherId,
    Guid[]? AuthorIds,
    string? Search) : IQuery<PagedResult<IEnumerable<Book>>>;

public sealed class ListBooksHandler(IAiService aiService, IReadRepository<Book> repository)
    : IQueryHandler<ListBooksQuery, PagedResult<IEnumerable<Book>>>
{
    public async Task<PagedResult<IEnumerable<Book>>> Handle(ListBooksQuery request,
        CancellationToken cancellationToken)
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
            vector);

        var books = await repository.ListAsync(spec, cancellationToken);

        var totalRecords = await repository.CountAsync(spec, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        PagedInfo pagedInfo = new(request.PageIndex, request.PageSize, totalPages, totalRecords);

        return new(pagedInfo, books);
    }
}
