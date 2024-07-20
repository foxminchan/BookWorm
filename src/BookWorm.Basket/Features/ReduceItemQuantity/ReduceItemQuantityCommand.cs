using System.Security.Claims;
using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Basket.Infrastructure.Redis;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Basket.Features.ReduceItemQuantity;

public sealed record ReduceItemQuantityCommand(Guid BookId) : ICommand<Result>;

public sealed class UpdateBasketHandler(IRedisService redisService, IHttpContextAccessor httpContext)
    : ICommandHandler<ReduceItemQuantityCommand, Result>
{
    public async Task<Result> Handle(ReduceItemQuantityCommand command, CancellationToken cancellationToken)
    {
        var customerId = httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier); ;

        Guard.Against.NullOrEmpty(customerId);

        var basket = await redisService.HashGetAsync<Domain.Basket?>(nameof(Basket), customerId);

        Guard.Against.NotFound(customerId, basket);

        basket.ReduceItemQuantity(command.BookId, 1);

        await redisService.HashSetAsync(nameof(Basket), customerId, basket);

        return Result.Success();
    }
}
