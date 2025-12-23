using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Rating;

public partial class ReviewForm
{
    [Parameter]
    public string? FirstName { get; set; }

    [Parameter]
    public EventCallback<string?> FirstNameChanged { get; set; }

    [Parameter]
    public string? LastName { get; set; }

    [Parameter]
    public EventCallback<string?> LastNameChanged { get; set; }

    [Parameter]
    public string? Comment { get; set; }

    [Parameter]
    public EventCallback<string?> CommentChanged { get; set; }

    [Parameter]
    public int Rating { get; set; }

    [Parameter]
    public EventCallback<int> RatingChanged { get; set; }

    [Parameter]
    public EventCallback OnSubmitReview { get; set; }

    private bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(FirstName)
            && !string.IsNullOrWhiteSpace(LastName)
            && !string.IsNullOrWhiteSpace(Comment)
            && Rating > 0;
    }

    private async Task OnSubmit()
    {
        if (IsValid())
        {
            await OnSubmitReview.InvokeAsync();
        }
    }
}
