using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;

public sealed class DeleteCategoryHandler(ICategoryRepository repository)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(category, request.Id);

        repository.Delete(category);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
