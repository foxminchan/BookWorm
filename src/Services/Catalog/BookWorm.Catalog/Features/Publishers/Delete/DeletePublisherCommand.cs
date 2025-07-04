namespace BookWorm.Catalog.Features.Publishers.Delete;

public sealed record DeletePublisherCommand(Guid Id) : ICommand;

public sealed class DeletePublisherHandler(IPublisherRepository repository)
    : ICommandHandler<DeletePublisherCommand>
{
    public async Task<Unit> Handle(
        DeletePublisherCommand request,
        CancellationToken cancellationToken
    )
    {
        var publisher = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(publisher, request.Id);

        repository.Delete(publisher);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
