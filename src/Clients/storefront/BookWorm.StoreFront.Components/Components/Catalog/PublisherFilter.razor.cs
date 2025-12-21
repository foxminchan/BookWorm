using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public sealed partial class PublisherFilter : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public required List<Publisher> Publishers { get; set; }

    [Parameter]
    public HashSet<Guid> SelectedPublisherIds { get; set; } = [];

    [Parameter]
    public EventCallback<(Guid PublisherId, bool IsChecked)> OnPublisherToggled { get; set; }

    private async Task HandlePublisherToggle(Guid publisherId, bool isChecked)
    {
        await OnPublisherToggled.InvokeAsync((publisherId, isChecked));
    }
}
