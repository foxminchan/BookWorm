namespace BookWorm.SharedKernel.Results;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    long TotalItems,
    double TotalPages
)
{
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
