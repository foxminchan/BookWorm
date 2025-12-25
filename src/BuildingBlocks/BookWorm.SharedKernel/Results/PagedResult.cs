namespace BookWorm.SharedKernel.Results;

public sealed class PagedResult<T> : List<T>
{
    public int PageIndex { get; }
    public int PageSize { get; }
    public long TotalItems { get; }
    public long TotalPages { get; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int pageIndex, int pageSize, long totalItems)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (long)Math.Ceiling((double)TotalItems / PageSize);

        AddRange(items);
    }
}
