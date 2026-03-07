using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Extensions;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using Mediator;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed record CreateOrderCommand : ICommand<Guid>;

public sealed class CreateOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal,
    IDistributedAccessLockProvider lockProvider,
    IBasketService basketService,
    IBookService bookService
) : ICommandHandler<CreateOrderCommand, Guid>
{
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

        List<OrderItem> orderItems =
        [
            .. basketItems.Items.Select(item =>
            {
                var book = response?.Books.FirstOrDefault(b => b.Id == item.Id);

                Guard.Against.NotFound(book, item.Id);

                var bookPrice = book.PriceSale ?? book.Price;

                return new OrderItem(Guid.Parse(book.Id), item.Quantity, (decimal)bookPrice!);
            }),
        ];

        var order = new Order(userId, null, orderItems);

        Order result;
        await using (
            var handle = await lockProvider.TryAcquireAsync(
                userId.ToString(),
                TimeSpan.FromMinutes(1),
                cancellationToken
            )
        )
        {
            if (handle.IsAcquired)
            {
                result = await repository.AddAsync(order, cancellationToken);
                await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Other process is already creating an order");
            }
        }

        return result.Id;
    }
}
