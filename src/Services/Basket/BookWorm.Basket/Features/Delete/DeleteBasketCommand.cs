namespace BookWorm.Basket.Features.Delete;

public sealed record DeleteBasketCommand : ICommand;

public sealed class DeleteBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<DeleteBasketCommand>
{
    public async Task<Unit> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        var sub = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);

        var userId = Guard.Against.NotAuthenticated(sub);

        var basket = await basketRepository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, userId);

        await basketRepository.DeleteBasketAsync(userId);

        return Unit.Value;
    }
}
