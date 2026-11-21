namespace BookWorm.Rating;

internal static class RatingCacheKeys
{
    public static string FeedbackTag(Guid bookId)
    {
        return $"rating:feedbacks:{bookId}";
    }

    public static string FeedbackList(
        Guid bookId,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool isDescending
    )
    {
        var normalizedOrderBy = string.IsNullOrWhiteSpace(orderBy)
            ? string.Empty
            : orderBy.Trim().ToLowerInvariant();

        return $"rating:feedbacks:v1:{bookId}:p{pageIndex}:s{pageSize}:o{normalizedOrderBy}:d{isDescending}";
    }

    public static string Summary(Guid bookId)
    {
        return $"rating:summary:v1:{bookId}";
    }
}
