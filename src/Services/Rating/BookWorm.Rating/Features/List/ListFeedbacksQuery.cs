using System.ComponentModel;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.SharedKernel.SeedWork.Model;

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

public sealed class ListFeedbacksHandler(IFeedbackRepository repository)
    : IQueryHandler<ListFeedbacksQuery, PagedResult<FeedbackDto>>
{
    public async Task<PagedResult<FeedbackDto>> Handle(
        ListFeedbacksQuery request,
        CancellationToken cancellationToken
    )
    {
        var filterSpec = new FeedbackFilterSpec(
            request.BookId,
            request.OrderBy,
            request.IsDescending,
            request.PageIndex,
            request.PageSize
        );
        var feedbacks = await repository.ListAsync(filterSpec, cancellationToken);

        var countSpec = new FeedbackFilterSpec(
            request.BookId,
            request.OrderBy,
            request.IsDescending
        );

        var totalItems = await repository.CountAsync(countSpec, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        return new(
            feedbacks.ToFeedbackDtos(),
            request.PageIndex,
            request.PageSize,
            totalItems,
            totalPages
        );
    }
}
