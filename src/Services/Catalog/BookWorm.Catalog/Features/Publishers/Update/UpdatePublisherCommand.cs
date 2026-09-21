using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Catalog.Features.Publishers.Update;

public sealed record UpdatePublisherCommand(PublisherId Id, string Name) : ICommand;

internal sealed class UpdatePublisherHandler(IPublisherRepository repository)
    : ICommandHandler<UpdatePublisherCommand>
{
    public async ValueTask<Unit> Handle(
        UpdatePublisherCommand request,
        CancellationToken cancellationToken
    )
    {
        var publisher = await repository.GetByIdAsync((Guid)request.Id, cancellationToken);

        Guard.Against.NotFound(publisher, (Guid)request.Id);

        publisher.UpdateName(request.Name);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
