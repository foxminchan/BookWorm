namespace BookWorm.Basket.Features.Update;

public sealed record UpdateBasketCommand(List<BasketItemRequest> Items) : ICommand;

public sealed class UpdateBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<UpdateBasketCommand>
{
    public async Task<Unit> Handle(UpdateBasketCommand request, CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var basket = await basketRepository.GetBasketAsync(userId);

        if (basket is null)
        {
            throw new NotFoundException($"Basket with id {userId} not found.");
        }

        basket.Update(
            request.Items.Select(item => new BasketItem(item.Id, item.Quantity)).ToList()
        );

        await basketRepository.UpdateBasketAsync(basket);

        return Unit.Value;
    }
}
