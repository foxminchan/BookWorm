using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed record CreateBuyerCommand([PIIData] string Street, string City, string Province)
    : ICommand<Guid>;

internal sealed class CreateBuyerHandler(
    IBuyerRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CreateBuyerCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        CreateBuyerCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

        var name = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Name) ?? "anonymous";

        var buyer = new Buyer(userId, name, request.Street, request.City, request.Province);

        var result = await repository.AddAsync(buyer, cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
