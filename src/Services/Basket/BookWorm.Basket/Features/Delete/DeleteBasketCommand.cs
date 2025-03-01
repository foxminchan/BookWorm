namespace BookWorm.Basket.Features.Delete;

public sealed record DeleteBasketCommand : ICommand;

public sealed class DeleteBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<DeleteBasketCommand>
{
    public async Task<Unit> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
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

        await basketRepository.DeleteBasketAsync(userId);

        return Unit.Value;
    }
}
