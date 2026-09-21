using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed record DeleteAuthorCommand(AuthorId Id) : ICommand;

internal sealed class DeleteAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<DeleteAuthorCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteAuthorCommand request,
        CancellationToken cancellationToken
    )
    {
        var author = await repository.GetByIdAsync((Guid)request.Id, cancellationToken);

        Guard.Against.NotFound(author, (Guid)request.Id);

        repository.Delete(author);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
