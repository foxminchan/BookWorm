using BookWorm.Ordering.Helpers;

namespace BookWorm.Ordering.Features.Buyers.UpdateAddress;

public sealed record UpdateAddressCommand(string Street, string City, string Province)
    : ICommand<BuyerDto>;

public sealed class UpdateAddressHandler(
    IBuyerRepository buyerRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<UpdateAddressCommand, BuyerDto>
{
    public async Task<BuyerDto> Handle(
        UpdateAddressCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToBuyerId();

        var buyer = await buyerRepository.GetByIdAsync(userId, cancellationToken);

        Guard.Against.NotFound(buyer, $"Buyer with id {userId} not found.");

        buyer.UpdateAddress(request.Street, request.City, request.Province);

        await buyerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return buyer.ToBuyerDto();
    }
}
