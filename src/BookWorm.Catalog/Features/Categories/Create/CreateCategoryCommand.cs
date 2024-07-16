using Ardalis.Result;
using BookWorm.Catalog.Domain;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Categories.Create;

public sealed record CreateCategoryCommand(string Name) : ICommand<Result<Guid>>;

public sealed class CreateCategoryHandler(IRepository<Category> repository)
    : ICommandHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        return result.Id;
    }
}
