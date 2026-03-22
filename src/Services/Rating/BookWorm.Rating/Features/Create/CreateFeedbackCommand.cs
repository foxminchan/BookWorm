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
) : ICommand<Guid>;

internal sealed class CreateFeedbackHandler(IFeedbackRepository repository)
    : ICommandHandler<CreateFeedbackCommand, Guid>
{
    public async ValueTask<Guid> Handle(
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
