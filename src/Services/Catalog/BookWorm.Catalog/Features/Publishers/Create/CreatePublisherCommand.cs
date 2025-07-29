using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed record CreatePublisherCommand(string Name) : ICommand<Guid>;

public sealed class CreatePublisherHandler(IPublisherRepository repository)
    : ICommandHandler<CreatePublisherCommand, Guid>
{
    public async Task<Guid> Handle(
        CreatePublisherCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        return result.Id;
    }
}
