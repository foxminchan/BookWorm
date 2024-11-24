namespace BookWorm.Rating.Features.Hide;

public sealed record HideFeedbackCommand(ObjectId Id) : ICommand<Result>;

public sealed class HideFeedbackHandler(IRatingRepository repository)
    : ICommandHandler<HideFeedbackCommand, Result>
{
    public async Task<Result> Handle(
        HideFeedbackCommand request,
        CancellationToken cancellationToken
    )
    {
        var filter = Builders<Feedback>.Filter.Eq(x => x.Id, request.Id);

        var feedback = await repository.GetAsync(filter, cancellationToken);

        Guard.Against.NotFound(request.Id, feedback);

        feedback.Hide();

        await repository.UpdateAsync(filter, feedback, cancellationToken);

        return Result.Success();
    }
}
