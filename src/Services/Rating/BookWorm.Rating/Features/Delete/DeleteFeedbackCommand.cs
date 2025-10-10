using BookWorm.Chassis.Guards;
using Mediator;

namespace BookWorm.Rating.Features.Delete;

public sealed record DeleteFeedbackCommand(Guid Id) : ICommand;

public sealed class DeleteFeedbackHandler(IFeedbackRepository repository)
    : ICommandHandler<DeleteFeedbackCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteFeedbackCommand request,
        CancellationToken cancellationToken
    )
    {
        var feedback = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(feedback, request.Id);

        feedback.Remove();

        repository.Delete(feedback);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
