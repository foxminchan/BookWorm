using BookWorm.Basket.Exceptions;

namespace BookWorm.Basket.Features.Create;

public sealed record CreateBasketCommand(List<BasketItemRequest> Items) : ICommand<string>;

public sealed class CreateBasketHandler(
    IBasketRepository basketRepository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CreateBasketCommand, string>
{
    public async Task<string> Handle(
        CreateBasketCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var basket = new CustomerBasket(
            userId,
            [.. request.Items.Select(i => new BasketItem(i.Id, i.Quantity))]
        );

        var result = await basketRepository.UpdateBasketAsync(basket);

        if (result?.Id is null)
        {
            throw new BasketCreatedException("An error occurred while creating the basket.");
        }

        return result.Id;
    }
}
