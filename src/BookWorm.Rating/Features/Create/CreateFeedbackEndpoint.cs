using BookWorm.Rating.Domain;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;

namespace BookWorm.Rating.Features.Create;

public sealed record CreateFeedbackRequest(Guid BookId, int Rating, string? Comment);

public sealed class CreateFeedbackEndpoint : IEndpoint<Created<ObjectId>, CreateFeedbackRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/feedbacks", async (CreateFeedbackRequest request, ISender sender) =>
                await HandleAsync(request, sender))
            .Produces<Created<ObjectId>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithTags(nameof(Feedback))
            .WithName("Create Feedback")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();

    public async Task<Created<ObjectId>> HandleAsync(CreateFeedbackRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateFeedbackCommand(request.BookId, request.Rating, request.Comment);

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/api/v1/feedbacks/{result.Value}", result.Value);
    }
}
