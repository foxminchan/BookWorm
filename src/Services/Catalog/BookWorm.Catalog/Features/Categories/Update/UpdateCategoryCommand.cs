namespace BookWorm.Catalog.Features.Categories.Update;

public sealed record UpdateCategoryCommand(Guid Id, string Name) : ICommand;

public sealed class UpdateCategoryHandler(ICategoryRepository repository)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Unit> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(category, request.Id);

        category.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
