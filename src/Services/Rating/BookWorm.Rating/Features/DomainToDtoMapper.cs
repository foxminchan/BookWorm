namespace BookWorm.Rating.Features;

internal static class DomainToDtoMapper
{
    extension(Feedback feedback)
    {
        private FeedbackDto ToFeedbackDto()
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
    }

    extension(IEnumerable<Feedback> feedbacks)
    {
        public IReadOnlyList<FeedbackDto> ToFeedbackDtos()
        {
            return [.. feedbacks.Select(ToFeedbackDto)];
        }
    }
}
