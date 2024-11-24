using BasketModel = BookWorm.Basket.Domain.Basket;

namespace BookWorm.Basket.Features.ReduceItemQuantity;

public sealed record ReduceItemQuantityCommand(Guid BookId) : ICommand<Result>;

public sealed class ReduceItemQuantityHandler(
    IRedisService redisService,
    IIdentityService identityService
) : ICommandHandler<ReduceItemQuantityCommand, Result>
{
    public async Task<Result> Handle(
        ReduceItemQuantityCommand command,
        CancellationToken cancellationToken
    )
    {
        var customerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(customerId);

        var basket = await redisService.HashGetAsync<BasketModel?>(nameof(Basket), customerId);

        Guard.Against.NotFound(customerId, basket);

        basket.ReduceItemQuantity(command.BookId, 1);

        await redisService.HashSetAsync(nameof(Basket), customerId, basket);

        return Result.Success();
    }
}
