using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.SharedKernel.Results;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Rating.Features.List;

public sealed record ListFeedbacksQuery(
    [property: Description("The ID of the book to get feedback for")] Guid BookId,
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageIndex)]
        int PageIndex = Pagination.DefaultPageIndex,
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageSize)]
        int PageSize = Pagination.DefaultPageSize,
    [property: Description("Property to order results by")]
    [property: DefaultValue(nameof(Feedback.Rating))]
        string? OrderBy = nameof(Feedback.Rating),
    [property: Description("Whether to order results in descending order")]
    [property: DefaultValue(false)]
        bool IsDescending = false
) : IQuery<PagedResult<FeedbackDto>>;

internal sealed record CachedFeedbackPage(IReadOnlyList<FeedbackDto> Items, long TotalItems);

internal sealed class ListFeedbacksHandler(IFeedbackRepository repository, IFusionCache cache)
    : IQueryHandler<ListFeedbacksQuery, PagedResult<FeedbackDto>>
{
    public async ValueTask<PagedResult<FeedbackDto>> Handle(
        ListFeedbacksQuery request,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = RatingCacheKeys.FeedbackList(
            request.BookId,
            request.PageIndex,
            request.PageSize,
            request.OrderBy,
            request.IsDescending
        );

        var page = await cache.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var filterSpec = new FeedbackFilterSpec(
                    request.BookId,
                    request.OrderBy,
                    request.IsDescending,
                    request.PageIndex,
                    request.PageSize
                );

                var feedbacks = await repository.ListAsync(filterSpec, ct);

                var countSpec = new FeedbackFilterSpec(request.BookId);

                var totalItems = await repository.CountAsync(countSpec, ct);

                return new CachedFeedbackPage(feedbacks.ToFeedbackDtos(), totalItems);
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(10),
                DistributedCacheDuration = TimeSpan.FromMinutes(10),
            },
            [RatingCacheKeys.FeedbackTag(request.BookId)],
            cancellationToken
        );

        return [with(page.Items, request.PageIndex, request.PageSize, page.TotalItems)];
    }
}
