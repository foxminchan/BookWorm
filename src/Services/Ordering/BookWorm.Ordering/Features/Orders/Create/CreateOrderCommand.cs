using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Extensions;
using Mediator;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed;

namespace BookWorm.Ordering.Features.Orders.Create;

[Transactional]
public sealed record CreateOrderCommand : ICommand<Guid>;

internal sealed class CreateOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal,
    IFusionCacheDistributedLocker distributedLocker,
    ILogger<CreateOrderHandler> logger,
    IBasketService basketService,
    IBookService bookService
) : ICommandHandler<CreateOrderCommand, Guid>
{
    private const string CacheName = "ordering";

    public async ValueTask<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

        var basketItems = await basketService.GetBasket(cancellationToken);

        var response = await bookService.GetBooksByIdsAsync(
            basketItems.Items.Select(item => item.Id),
            cancellationToken
        );

        var booksById = response?.Books.ToDictionary(b => b.Id) ?? [];

        List<OrderItem> orderItems =
        [
            .. basketItems.Items.Select(item =>
            {
                booksById.TryGetValue(item.Id, out var book);

                Guard.Against.NotFound(book, item.Id);

                var bookPrice = book.PriceSale ?? book.Price;

                return new OrderItem(Guid.Parse(book.Id), item.Quantity, (decimal)bookPrice!);
            }),
        ];

        var order = new Order(userId, null, orderItems);

        var userIdStr = userId.ToString();
        var lockName = $"order-create:{userIdStr}";

        var locker = await distributedLocker.AcquireLockAsync(
            CacheName,
            string.Empty,
            string.Empty,
            userIdStr,
            lockName,
            TimeSpan.FromMinutes(1),
            logger,
            cancellationToken
        );

        try
        {
            if (locker is null)
            {
                throw new InvalidOperationException("Other process is already creating an order");
            }

            var result = await repository.AddAsync(order, cancellationToken);
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return result.Id;
        }
        finally
        {
            await distributedLocker.ReleaseLockAsync(
                CacheName,
                string.Empty,
                string.Empty,
                userIdStr,
                lockName,
                locker,
                logger,
                cancellationToken
            );
        }
    }
}
