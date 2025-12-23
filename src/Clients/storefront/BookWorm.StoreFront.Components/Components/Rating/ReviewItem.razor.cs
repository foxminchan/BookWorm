using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Rating;

public partial class ReviewItem
{
    [Parameter, EditorRequired]
    public required Feedback Review { get; set; }
}
