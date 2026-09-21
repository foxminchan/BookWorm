using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Authors.Update;

public sealed record UpdateAuthorCommand(AuthorId Id, string Name) : ICommand;

internal sealed class UpdateAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<UpdateAuthorCommand>
{
    public async ValueTask<Unit> Handle(
        UpdateAuthorCommand request,
        CancellationToken cancellationToken
    )
    {
        var author = await repository.GetByIdAsync((Guid)request.Id, cancellationToken);

        Guard.Against.NotFound(author, (Guid)request.Id);

        author.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
