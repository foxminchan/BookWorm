using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Rating.Domain;
using BookWorm.Rating.IntegrationEvents.Events;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookWorm.Rating.Features.Create;

public sealed record CreateFeedbackCommand(Guid BookId, int Rating, string? Comment, Guid UserId)
    : ICommand<Result<ObjectId>>;

public sealed class CreateFeedbackHandler(IMongoCollection<Feedback> collection, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateFeedbackCommand, Result<ObjectId>>
{
    public async Task<Result<ObjectId>> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = new Feedback(request.BookId, request.Rating, request.Comment, request.UserId);

        await collection.InsertOneAsync(feedback, cancellationToken: cancellationToken);

        var @event = new FeedbackCreatedIntegrationEvent(feedback.Id.ToString(), feedback.BookId, feedback.Rating);

        await publishEndpoint.Publish(@event, cancellationToken);

        return feedback.Id;
    }
}
