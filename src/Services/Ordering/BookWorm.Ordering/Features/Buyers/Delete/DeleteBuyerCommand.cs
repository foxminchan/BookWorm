using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Ordering.Features.Buyers.Delete;

public sealed record DeleteBuyerCommand(Guid Id) : ICommand;

public sealed class DeleteBuyerHandler(IBuyerRepository repository)
    : ICommandHandler<DeleteBuyerCommand>
{
    public async Task<Unit> Handle(DeleteBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(buyer, request.Id);

        repository.Delete(buyer);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
