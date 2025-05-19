namespace BookWorm.Rating.Features;

public static class DomainToDtoMapper
{
    private static FeedbackDto ToFeedbackDto(this Feedback feedback)
    {
        return new(
            feedback.Id,
            feedback.FirstName,
            feedback.LastName,
            feedback.Comment,
            feedback.Rating,
            feedback.BookId
        );
    }

    public static IReadOnlyList<FeedbackDto> ToFeedbackDtos(this IEnumerable<Feedback> feedbacks)
    {
        return [.. feedbacks.AsValueEnumerable().Select(ToFeedbackDto)];
    }
}
