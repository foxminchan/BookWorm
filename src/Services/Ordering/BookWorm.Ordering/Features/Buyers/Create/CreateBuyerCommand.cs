using BookWorm.Chassis.CQRS;
using BookWorm.Ordering.Extensions;
using Microsoft.Identity.Web;

namespace BookWorm.Ordering.Features.Buyers.Create;

[Transactional]
public sealed record CreateBuyerCommand(
    [property: PIIData] string Street,
    [property: PIIData] string City,
    [property: PIIData] string Province
) : ICommand<BuyerId>;

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

        var name = claimsPrincipal[ClaimConstants.Name] ?? "anonymous";

        var buyer = new Buyer(userId, name, request.Street, request.City, request.Province);

        var result = await repository.AddAsync(buyer, cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
