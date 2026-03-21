using BookWorm.Basket.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Basket.Features.Delete;

public sealed record DeleteBasketCommand : ICommand;

internal sealed class DeleteBasketHandler(
    IBasketRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<DeleteBasketCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteBasketCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var basket = await repository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, userId);

        await repository.DeleteBasketAsync(userId);

        return Unit.Value;
    }
}
