using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Base;

public sealed partial class Breadcrumb
{
    [Parameter]
    [EditorRequired]
    public required List<BreadcrumbItem> Items { get; set; }

    public sealed record BreadcrumbItem(string Label, string? Url = null);
}
