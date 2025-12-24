using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Pagination;

public sealed partial class Pagination
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int TotalPages { get; set; }

    [Parameter]
    public Action<int>? OnPageChanged { get; set; }

    private void HandlePageChange(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            OnPageChanged?.Invoke(page);
        }
    }
}
