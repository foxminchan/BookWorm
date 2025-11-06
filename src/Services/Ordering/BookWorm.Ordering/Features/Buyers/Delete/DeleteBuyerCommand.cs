using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Delete;

public sealed record DeleteBuyerCommand(Guid Id) : ICommand;

public sealed class DeleteBuyerHandler(IBuyerRepository repository)
    : ICommandHandler<DeleteBuyerCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteBuyerCommand request,
        CancellationToken cancellationToken
    )
    {
        var buyer = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(buyer, request.Id);

        repository.Delete(buyer);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
