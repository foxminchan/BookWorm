using BookWorm.Ordering.Helpers;

namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed record CreateBuyerCommand(string Street, string City, string Province)
    : ICommand<Guid>;

public sealed class CreateBuyerHandler(
    IBuyerRepository buyerRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CreateBuyerCommand, Guid>
{
    public async Task<Guid> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToBuyerId();

        var name = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Name) ?? "anonymous";

        var buyer = new Buyer(userId, name, request.Street, request.City, request.Province);

        var result = await buyerRepository.AddAsync(buyer, cancellationToken);

        await buyerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
