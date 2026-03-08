using BookWorm.Basket.Extensions;
using Mediator;

namespace BookWorm.Basket.Features.Create;

public sealed record CreateBasketCommand(List<BasketItemRequest> Items) : ICommand<string>;

internal sealed class CreateBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CreateBasketCommand, string>
{
    public async ValueTask<string> Handle(
        CreateBasketCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var basket = new CustomerBasket(userId, request.Items.ToBasketItem());

        var result = await basketRepository.CreateOrUpdateBasketAsync(basket);

        return result?.Id
            ?? throw new BasketCreatedException("An error occurred while creating the basket.");
    }
}
