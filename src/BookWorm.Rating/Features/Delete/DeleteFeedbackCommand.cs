using BookWorm.Contracts;

namespace BookWorm.Rating.Features.Delete;

public sealed record DeleteFeedbackCommand(ObjectId Id) : ICommand<Result>;

public sealed class DeleteFeedbackHandler(
    IRatingRepository repository,
    IPublishEndpoint publishEndpoint,
    IIdentityService identityService) : ICommandHandler<DeleteFeedbackCommand, Result>
{
    public async Task<Result> Handle(DeleteFeedbackCommand request, CancellationToken cancellationToken)
    {
        FilterDefinition<Feedback> filter;
        if (!identityService.IsAdminRole())
        {
            filter = Builders<Feedback>.Filter.And(
                Builders<Feedback>.Filter.Eq(x => x.Id, request.Id),
                Builders<Feedback>.Filter.Eq(x => x.UserId, Guid.Parse(identityService.GetUserIdentity()!))
            );
        }
        else
        {
            filter = Builders<Feedback>.Filter.Eq(x => x.Id, request.Id);
        }

        var feedback = await repository.GetAsync(filter, cancellationToken);

        Guard.Against.NotFound(request.Id, feedback);

        await repository.DeleteAsync(filter, cancellationToken);

        var @event = new FeedbackDeletedIntegrationEvent(feedback.Id.ToString(), feedback.BookId, feedback.Rating);

        await publishEndpoint.Publish(@event, cancellationToken);

        return Result.Success();
    }
}
