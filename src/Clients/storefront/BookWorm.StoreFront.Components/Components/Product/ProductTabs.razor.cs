using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Product;

public partial class ProductTabs
{
    [Parameter]
    public string ActiveTab { get; set; } = "description";

    [Parameter]
    public EventCallback<string> ActiveTabChanged { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public int TotalReviews { get; set; }

    [Parameter, EditorRequired]
    public required Guid BookId { get; set; }

    [Parameter]
    public string? BookName { get; set; }

    private string? _reviewFirstName;
    private string? _reviewLastName;
    private string? _reviewComment;
    private int _reviewRating;
    private int _refreshTrigger;

    private async Task SetActiveTab(string tab)
    {
        ActiveTab = tab;
        await ActiveTabChanged.InvokeAsync(tab);
    }

    private void HandleReviewSubmit()
    {
        if (!IsReviewValid())
            return;

        // Create new feedback
        var feedback = new Feedback(
            Guid.NewGuid(),
            _reviewFirstName,
            _reviewLastName,
            _reviewComment,
            _reviewRating,
            BookId
        );

        // Reset form
        _reviewFirstName = null;
        _reviewLastName = null;
        _reviewComment = null;
        _reviewRating = 0;

        // Trigger refresh of reviews list
        _refreshTrigger++;

        // TODO: Send to backend API
        Console.WriteLine(
            $"Review submitted for book {BookName} by {feedback.FirstName} {feedback.LastName}"
        );
    }

    private bool IsReviewValid()
    {
        return !string.IsNullOrWhiteSpace(_reviewFirstName)
            && !string.IsNullOrWhiteSpace(_reviewLastName)
            && !string.IsNullOrWhiteSpace(_reviewComment)
            && _reviewRating > 0;
    }
}
