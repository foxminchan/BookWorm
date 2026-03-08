using BookWorm.Basket.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Basket.Features.Delete;

public sealed record DeleteBasketCommand : ICommand;

internal sealed class DeleteBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<DeleteBasketCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteBasketCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var basket = await basketRepository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, userId);

        await basketRepository.DeleteBasketAsync(userId);

        return Unit.Value;
    }
}
