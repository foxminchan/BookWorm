using Mediator;

namespace BookWorm.Catalog.Features.Authors.Create;

public sealed record CreateAuthorCommand(string Name) : ICommand<AuthorId>;

internal sealed class CreateAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<CreateAuthorCommand, AuthorId>
{
    public async ValueTask<AuthorId> Handle(
        CreateAuthorCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
