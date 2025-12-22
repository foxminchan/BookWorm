using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Base;

public sealed partial class CollapsibleFilterSection
{
    private bool _isExpanded = true;

    [Parameter]
    [EditorRequired]
    public required string Title { get; set; }

    [Parameter]
    public bool InitiallyExpanded { get; set; } = true;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        _isExpanded = InitiallyExpanded;
    }

    private void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
    }
}
