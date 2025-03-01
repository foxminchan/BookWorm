namespace BookWorm.Catalog.Features.Categories.Update;

public sealed record UpdateCategoryCommand([property: JsonIgnore] Guid Id, string Name) : ICommand;

public sealed class UpdateCategoryHandler(ICategoryRepository repository)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Unit> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with id {request.Id} not found.");
        }

        category.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
