using Mediator;

namespace BookWorm.Catalog.Features.Categories.Create;

public sealed record CreateCategoryCommand(string Name) : ICommand<Guid>;

public sealed class CreateCategoryHandler(ICategoryRepository repository)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
