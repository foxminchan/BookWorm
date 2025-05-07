namespace BookWorm.SharedKernel.Results;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    long TotalItems
)
{
    public long TotalPages => (long)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
