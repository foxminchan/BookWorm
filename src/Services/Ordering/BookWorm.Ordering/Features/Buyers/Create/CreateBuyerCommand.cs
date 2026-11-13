using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Ordering.Extensions;

namespace BookWorm.Ordering.Features.Buyers.Create;

[Transactional]
public sealed record CreateBuyerCommand([PIIData] string Street, string City, string Province)
    : ICommand<BuyerId>;

internal sealed class CreateBuyerHandler(
    IBuyerRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CreateBuyerCommand, BuyerId>
{
    public async ValueTask<BuyerId> Handle(
        CreateBuyerCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal[ClaimTypes.NameIdentifier].ToBuyerId();

        var name = claimsPrincipal[KeycloakClaimTypes.Name] ?? "anonymous";

        var buyer = new Buyer(userId, name, request.Street, request.City, request.Province);

        var result = await repository.AddAsync(buyer, cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
