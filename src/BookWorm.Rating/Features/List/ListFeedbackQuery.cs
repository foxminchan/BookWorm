using Ardalis.Result;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Rating.Features.List;

public sealed record ListFeedbackQuery(Guid BookId, int PageIndex, int PageSize)
    : IQuery<PagedResult<IEnumerable<Feedback>>>;

public sealed class ListFeedbackHandler(IMongoCollection<Feedback> collection)
    : IQueryHandler<ListFeedbackQuery, PagedResult<IEnumerable<Feedback>>>
{
    public async Task<PagedResult<IEnumerable<Feedback>>> Handle(ListFeedbackQuery request,
        CancellationToken cancellationToken)
    {
        var filter = Builders<Feedback>.Filter.Eq(x => x.BookId, request.BookId);

        var feedbacks = await collection.Find(filter)
            .Skip(request.PageIndex * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        var totalRecords = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var pagedInfo = new PagedInfo(request.PageIndex, request.PageSize, totalPages, totalRecords);

        return new(pagedInfo, feedbacks);
    }
}
