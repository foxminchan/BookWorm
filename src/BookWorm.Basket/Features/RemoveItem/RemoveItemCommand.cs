using BasketModel = BookWorm.Basket.Domain.Basket;

namespace BookWorm.Basket.Features.RemoveItem;

public sealed record RemoveItemCommand(Guid BookId) : ICommand<Result>;

public sealed class RemoveItemHandler(IRedisService redisService, IIdentityService identityService)
    : ICommandHandler<RemoveItemCommand, Result>
{
    public async Task<Result> Handle(RemoveItemCommand command, CancellationToken cancellationToken)
    {
        var customerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(customerId);

        var basket = await redisService.HashGetAsync<BasketModel?>(nameof(Basket), customerId);

        Guard.Against.NotFound(customerId, basket);

        basket.RemoveItem(command.BookId);

        await redisService.HashSetAsync(nameof(Basket), customerId, basket);

        return Result.Success();
    }
}
