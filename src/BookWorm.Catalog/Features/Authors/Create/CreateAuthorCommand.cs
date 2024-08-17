using Ardalis.Result;
using BookWorm.Catalog.Domain;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Authors.Create;

public sealed record CreateAuthorCommand(string Name) : ICommand<Result<Guid>>;

public sealed class CreateAuthorHandler(IRepository<Author> repository)
    : ICommandHandler<CreateAuthorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        return result.Id;
    }
}
