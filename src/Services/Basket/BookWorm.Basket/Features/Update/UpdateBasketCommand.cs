namespace BookWorm.Basket.Features.Update;

public sealed record UpdateBasketCommand(List<BasketItemRequest> Items) : ICommand;

public sealed class UpdateBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<UpdateBasketCommand>
{
    public async Task<Unit> Handle(UpdateBasketCommand request, CancellationToken cancellationToken)
    {
        var sub = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);

        var userId = Guard.Against.NotAuthenticated(sub);

        var basket = await basketRepository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, userId);

        basket.Update(request.Items.ToBasketItem());

        await basketRepository.CreateOrUpdateBasketAsync(basket);

        return Unit.Value;
    }
}
