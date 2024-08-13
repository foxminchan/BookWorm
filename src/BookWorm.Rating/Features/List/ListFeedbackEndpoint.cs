using Ardalis.Result;
using BookWorm.Rating.Domain;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.Features.List;

public sealed record ListFeedbackRequest(Guid BookId, int PageIndex, int PageSize);

public sealed record ListFeedbackResponse(PagedInfo PagedInfo, List<Feedback> Feedbacks);

public sealed class ListFeedbackEndpoint : IEndpoint<Ok<ListFeedbackResponse>, ListFeedbackRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/feedbacks",
                async (Guid bookId, int pageIndex, int pageSize, ISender sender) =>
                    await HandleAsync(new(bookId, pageIndex, pageSize), sender))
            .Produces<Ok<ListFeedbackResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Feedback))
            .WithName("List Feedback")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();

    public async Task<Ok<ListFeedbackResponse>> HandleAsync(ListFeedbackRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var query = new ListFeedbackQuery(request.BookId, request.PageIndex, request.PageSize);

        var feedbacks = await sender.Send(query, cancellationToken);

        var response = new ListFeedbackResponse(feedbacks.PagedInfo, feedbacks.Value.ToList());

        return TypedResults.Ok(response);
    }
}
