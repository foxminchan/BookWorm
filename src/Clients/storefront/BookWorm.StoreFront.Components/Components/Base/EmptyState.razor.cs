using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Base;

public sealed partial class EmptyState
{
    [Parameter]
    public string Title { get; set; } = "No products found";

    [Parameter]
    public string Message { get; set; } =
        "We couldn't find any products matching your criteria. Try adjusting your filters or clear them to see all available products.";

    [Parameter]
    public bool ShowClearButton { get; set; } = true;

    [Parameter]
    public EventCallback OnClearFilters { get; set; }

    private async Task HandleClearFilters()
    {
        await OnClearFilters.InvokeAsync();
    }
}
