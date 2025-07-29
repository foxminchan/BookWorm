using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed record DeleteAuthorCommand(Guid Id) : ICommand;

public sealed class DeleteAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<DeleteAuthorCommand>
{
    public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(author, request.Id);

        repository.Delete(author);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
