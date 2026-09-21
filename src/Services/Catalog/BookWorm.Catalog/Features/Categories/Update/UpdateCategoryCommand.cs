using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Categories.Update;

public sealed record UpdateCategoryCommand(CategoryId Id, string Name) : ICommand;

internal sealed class UpdateCategoryHandler(ICategoryRepository repository)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async ValueTask<Unit> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = await repository.GetByIdAsync((Guid)request.Id, cancellationToken);

        Guard.Against.NotFound(category, (Guid)request.Id);

        category.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
