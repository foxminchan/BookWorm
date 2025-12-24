using BookWorm.StoreFront.Components.Enums;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Rating;

public sealed partial class Rating
{
    [Parameter]
    public double Value { get; set; }

    [Parameter]
    public int MaxStars { get; set; } = 5;

    [Parameter]
    public bool Interactive { get; set; }

    [Parameter]
    public RatingSize Size { get; set; } = RatingSize.Medium;

    [Parameter]
    public bool ShowCount { get; set; }

    [Parameter]
    public int? TotalCount { get; set; }

    [Parameter]
    public bool ShowValue { get; set; }

    [Parameter]
    public string? AdditionalClasses { get; set; }

    [Parameter]
    public EventCallback<double> OnRatingChange { get; set; }

    private int? _hoveredStar;

    private string GetAriaLabel()
    {
        if (Interactive)
        {
            return $"Rate from 1 to {MaxStars} stars, current rating {Value:F1}";
        }

        return $"Rating: {Value:F1} out of {MaxStars} stars"
            + (TotalCount.HasValue ? $" based on {TotalCount.Value} reviews" : string.Empty);
    }

    private string GetStarClasses(int starIndex, bool isInteractive = true)
    {
        var classes = new List<string>
        {
            // Size classes
            Size switch
            {
                RatingSize.Small => "text-sm",
                RatingSize.Medium => "text-base",
                RatingSize.Large => "text-xl",
                RatingSize.ExtraLarge => "text-2xl",
                _ => "text-base",
            },
        };

        // Interactive button styles
        if (isInteractive && Interactive)
        {
            classes.Add(
                "cursor-pointer transition-all duration-150 hover:scale-110 focus:outline-none focus:ring-2 focus:ring-primary rounded"
            );
        }

        // Color based on state
        var isFilled = starIndex <= Math.Floor(Value);
        var isHovered = _hoveredStar.HasValue && starIndex <= _hoveredStar.Value;

        if (isFilled || isHovered)
        {
            classes.Add("text-yellow-400");
        }
        else
        {
            classes.Add("text-base-300");
        }

        if (!string.IsNullOrWhiteSpace(AdditionalClasses))
        {
            classes.Add(AdditionalClasses);
        }

        return string.Join(" ", classes);
    }

    private string GetCountClasses()
    {
        var classes = new List<string>
        {
            Size switch
            {
                RatingSize.Small => "text-xs",
                RatingSize.Medium => "text-sm",
                RatingSize.Large => "text-base",
                RatingSize.ExtraLarge => "text-lg",
                _ => "text-sm",
            },
            "text-base-content/60 ml-1",
        };

        return string.Join(" ", classes);
    }

    private async Task HandleStarClick(int star)
    {
        if (!Interactive)
            return;

        Value = star;
        await OnRatingChange.InvokeAsync(Value);
    }

    private void HandleStarHover(int star)
    {
        if (!Interactive)
            return;
        _hoveredStar = star;
    }

    private void HandleMouseOut()
    {
        if (!Interactive)
            return;
        _hoveredStar = null;
    }
}
