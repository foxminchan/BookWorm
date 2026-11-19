namespace BookWorm.SharedKernel.Results;

public interface IPagedResult
{
    int PageIndex { get; }
    int PageSize { get; }
    long TotalItems { get; }
    long TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public sealed class PagedResult<T>(
    IReadOnlyList<T> items,
    int pageIndex,
    int pageSize,
    long totalItems
) : List<T>(items), IPagedResult
{
    public int PageIndex { get; } = pageIndex;
    public int PageSize { get; } = pageSize;
    public long TotalItems { get; } = totalItems;
    public long TotalPages { get; } = (long)Math.Ceiling((double)totalItems / pageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
