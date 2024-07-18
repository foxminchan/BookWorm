using Ardalis.Result;
using BookWorm.Catalog.Domain;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed record CreatePublisherCommand(string Name) : ICommand<Result<Guid>>;

public sealed class CreatePublisherHandler(IRepository<Publisher> repository)
    : ICommandHandler<CreatePublisherCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePublisherCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        return result.Id;
    }
}
