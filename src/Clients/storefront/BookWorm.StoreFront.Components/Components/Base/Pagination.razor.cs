using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Base;

public sealed partial class Pagination
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int TotalPages { get; set; }

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    private async Task HandlePageChange(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            await OnPageChanged.InvokeAsync(page);
        }
    }
}
