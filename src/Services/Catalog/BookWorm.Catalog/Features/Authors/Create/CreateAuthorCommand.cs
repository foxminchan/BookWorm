using Mediator;

namespace BookWorm.Catalog.Features.Authors.Create;

public sealed record CreateAuthorCommand(string Name) : ICommand<Guid>;

public sealed class CreateAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<CreateAuthorCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        CreateAuthorCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
