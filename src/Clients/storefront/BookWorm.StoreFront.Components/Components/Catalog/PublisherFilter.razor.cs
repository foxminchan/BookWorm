using BookWorm.StoreFront.Components.Mocks;
using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public sealed partial class PublisherFilter
{
    [Parameter]
    public HashSet<Guid> SelectedPublisherIds { get; set; } = [];

    [Parameter]
    public EventCallback<(Guid PublisherId, bool IsChecked)> OnPublisherToggled { get; set; }

    private List<Publisher> Publishers { get; set; } = [];
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        _isLoading = true;
        Publishers = MockDataProvider.GetPublishers();
        _isLoading = false;
    }

    private async Task HandlePublisherToggle(Guid publisherId, bool isChecked)
    {
        await OnPublisherToggled.InvokeAsync((publisherId, isChecked));
    }
}
