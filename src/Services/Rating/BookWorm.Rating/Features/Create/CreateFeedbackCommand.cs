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
        // Replace existing review from the same customer for the same book (upsert pattern)
        var existing = await repository.FindByBookAndCustomerAsync(
            request.BookId,
            request.FirstName,
            request.LastName,
            cancellationToken
        );

        if (existing is not null)
        {
            repository.Delete(existing.Remove());
        }

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
