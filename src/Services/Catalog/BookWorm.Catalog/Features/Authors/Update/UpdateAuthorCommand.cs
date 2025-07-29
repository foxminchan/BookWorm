using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Catalog.Features.Authors.Update;

public sealed record UpdateAuthorCommand(Guid Id, string Name) : ICommand;

public sealed class UpdateAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<UpdateAuthorCommand>
{
    public async Task<Unit> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(author, request.Id);

        author.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
