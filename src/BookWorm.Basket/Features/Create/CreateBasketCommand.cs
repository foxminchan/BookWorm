using System.Security.Claims;
using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Basket.Infrastructure.Redis;
using BookWorm.Core.SharedKernel;
using BookWorm.ServiceDefaults;

namespace BookWorm.Basket.Features.Create;

public sealed record CreateBasketCommand(Guid BookId, int Quantity) : ICommand<Result<Guid>>;

public sealed class CreateBasketHandler(IRedisService redisService, ClaimsPrincipal principal)
    : ICommandHandler<CreateBasketCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBasketCommand command, CancellationToken cancellationToken)
    {
        var customerId = principal.GetUserId();

        Guard.Against.NullOrEmpty(customerId);

        Domain.Basket basket = new(Guid.Parse(customerId), [new(command.BookId, command.Quantity)]);

        var existingBasket = await redisService.HashGetAsync<Domain.Basket?>(nameof(Basket), customerId);

        if (existingBasket is not null)
        {
            existingBasket.AddItem(new(command.BookId, command.Quantity));
            await redisService.HashSetAsync(nameof(Basket), customerId, existingBasket);
            return existingBasket.AccountId;
        }

        await redisService.HashSetAsync(nameof(Basket), customerId, basket);
        return basket.AccountId;
    }
}
