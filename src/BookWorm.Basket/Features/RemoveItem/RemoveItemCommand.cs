using System.Security.Claims;
using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Basket.Infrastructure.Redis;
using BookWorm.Core.SharedKernel;
using BookWorm.ServiceDefaults;

namespace BookWorm.Basket.Features.RemoveItem;

public sealed record RemoveItemCommand(Guid BookId) : ICommand<Result>;

public sealed class RemoveItemHandler(IRedisService redisService, ClaimsPrincipal principal)
    : ICommandHandler<RemoveItemCommand, Result>
{
    public async Task<Result> Handle(RemoveItemCommand command, CancellationToken cancellationToken)
    {
        var customerId = principal.GetUserId();

        Guard.Against.NullOrEmpty(customerId);

        var basket = await redisService.HashGetAsync<Domain.Basket?>(nameof(Basket), customerId);

        Guard.Against.NotFound(customerId, basket);

        basket.RemoveItem(command.BookId);

        await redisService.HashSetAsync(nameof(Basket), customerId, basket);

        return Result.Success();
    }
}
