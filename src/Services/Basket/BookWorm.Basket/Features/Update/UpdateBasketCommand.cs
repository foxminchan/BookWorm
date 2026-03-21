using BookWorm.Basket.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Basket.Features.Update;

public sealed record UpdateBasketCommand(List<BasketItemRequest> Items) : ICommand;

internal sealed class UpdateBasketHandler(
    IBasketRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<UpdateBasketCommand>
{
    public async ValueTask<Unit> Handle(
        UpdateBasketCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var basket = await repository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, userId);

        basket.Update(request.Items.ToBasketItem());

        await repository.CreateOrUpdateBasketAsync(basket);

        return Unit.Value;
    }
}
