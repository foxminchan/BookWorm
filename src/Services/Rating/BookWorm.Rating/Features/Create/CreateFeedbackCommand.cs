using BookWorm.Chassis.CQRS;
using Mediator;

namespace BookWorm.Rating.Features.Create;

[Transactional]
public sealed record CreateFeedbackCommand(
    Guid BookId,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating
) : ICommand<FeedbackId>;

internal sealed class CreateFeedbackHandler(IFeedbackRepository repository)
    : ICommandHandler<CreateFeedbackCommand, FeedbackId>
{
    public async ValueTask<FeedbackId> Handle(
        CreateFeedbackCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = await repository.AddAsync(
            new(
                request.BookId,
                request.FirstName,
                request.LastName,
                request.Comment,
                request.Rating
            ),
            cancellationToken
        );

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
