using BookWorm.Ordering.Helpers;
using Medallion.Threading;
using MediatR.Pipeline;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed record CreateOrderCommand : ICommand<Guid>
{
    [JsonIgnore]
    public List<OrderItem> Items { get; set; } = [];
}

public sealed class PreCreateOrderHandler([AsParameters] BasketMetadata basket)
    : IRequestPreProcessor<CreateOrderCommand>
{
    public async Task Process(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var basketItems = await basket.BasketService.GetBasket(cancellationToken);

        var response = await basket.BookService.GetBooksByIdsAsync(
            basketItems.Items.Select(item => item.Id),
            cancellationToken
        );

        request.Items =
        [
            .. basketItems.Items.Select(item =>
            {
                var book = response?.Books.FirstOrDefault(b => b.Id == item.Id);

                if (book is null)
                {
                    throw new NotFoundException($"Book with id {item.Id} not found");
                }

                var bookPrice = book.PriceSale ?? book.Price;

                return new OrderItem(Guid.Parse(book.Id), item.Quantity, (decimal)bookPrice);
            }),
        ];
    }
}

public sealed class CreateOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal,
    IDistributedLockProvider lockProvider
) : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToBuyerId();

        var order = new Order(userId, null, request.Items);

        Order result;
        await using (
            var handle = await lockProvider.TryAcquireLockAsync(
                userId.ToString(),
                TimeSpan.FromMinutes(1),
                cancellationToken: cancellationToken
            )
        )
        {
            if (handle is not null)
            {
                result = await repository.AddAsync(order, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Other process is already creating an order");
            }
        }

        return result.Id;
    }
}
