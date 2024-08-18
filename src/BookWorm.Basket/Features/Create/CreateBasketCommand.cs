using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Basket.Features.Create;

public sealed record CreateBasketCommand(Guid BookId, int Quantity) : ICommand<Result<Guid>>;

public sealed class CreateBasketHandler(
    IRedisService redisService,
    IIdentityService identityService) : ICommandHandler<CreateBasketCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBasketCommand command, CancellationToken cancellationToken)
    {
        var customerId = identityService.GetUserIdentity();

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
