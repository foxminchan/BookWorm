namespace BookWorm.Catalog.Features.Publishers.Update;

public sealed record UpdatePublisherCommand(Guid Id, string Name) : ICommand;

public sealed class UpdatePublisherHandler(IPublisherRepository repository)
    : ICommandHandler<UpdatePublisherCommand>
{
    public async Task<Unit> Handle(
        UpdatePublisherCommand request,
        CancellationToken cancellationToken
    )
    {
        var publisher = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (publisher is null)
        {
            throw new NotFoundException($"Publisher with id {request.Id} not found.");
        }

        publisher.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
