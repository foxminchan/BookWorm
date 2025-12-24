using BookWorm.StoreFront.Components.Mocks;
using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Filter;

public sealed partial class CategoryFilter
{
    [Parameter]
    public HashSet<Guid> SelectedCategoryIds { get; set; } = [];

    [Parameter]
    public Action<(Guid CategoryId, bool IsChecked)>? OnCategoryToggled { get; set; }

    private List<Category> Categories { get; set; } = [];
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        _isLoading = true;
        Categories = MockDataProvider.GetCategories();
        _isLoading = false;
    }

    private void HandleCategoryToggle(Guid categoryId, bool isChecked)
    {
        OnCategoryToggled?.Invoke((categoryId, isChecked));
    }

    private void HandleCategoryToggleInternal((Guid Id, bool IsChecked) args)
    {
        HandleCategoryToggle(args.Id, args.IsChecked);
    }
}
