using BookWorm.StoreFront.Components.Mocks;
using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Review;

public partial class ReviewsList
{
    [Parameter, EditorRequired]
    public required Guid BookId { get; set; }

    [Parameter]
    public int RefreshTrigger { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 5;

    private List<Feedback> _reviews = [];
    private List<Feedback> _pagedReviews = [];
    private bool _isLoading = true;
    private int _lastRefreshTrigger;
    private int _currentPage = 1;
    private int _totalPages;

    protected override async Task OnInitializedAsync()
    {
        await LoadReviewsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_lastRefreshTrigger != RefreshTrigger)
        {
            _lastRefreshTrigger = RefreshTrigger;
            _currentPage = 1; // Reset to first page when refreshing
            await LoadReviewsAsync();
        }
    }

    private async Task LoadReviewsAsync()
    {
        _isLoading = true;
        await Task.Delay(100); // Simulate async loading
        _reviews = MockDataProvider.GetFeedbacksByBookId(BookId);
        _totalPages = (int)Math.Ceiling(_reviews.Count / (double)PageSize);
        UpdatePagedReviews();
        _isLoading = false;
    }

    private void HandlePageChange(int page)
    {
        _currentPage = page;
        UpdatePagedReviews();
    }

    private void UpdatePagedReviews()
    {
        _pagedReviews = [.. _reviews.Skip((_currentPage - 1) * PageSize).Take(PageSize)];
    }
}
