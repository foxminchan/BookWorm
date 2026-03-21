using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.UpdateAddress;

public sealed record UpdateAddressCommand(string Street, string City, string Province)
    : ICommand<BuyerDto>;

internal sealed class UpdateAddressHandler(
    IBuyerRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<UpdateAddressCommand, BuyerDto>
{
    public async ValueTask<BuyerDto> Handle(
        UpdateAddressCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

        var buyer = await repository.GetByIdAsync(userId, cancellationToken);

        Guard.Against.NotFound(buyer, userId);

        buyer.UpdateAddress(request.Street, request.City, request.Province);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return buyer.ToBuyerDto();
    }
}
