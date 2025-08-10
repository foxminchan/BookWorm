using BookWorm.Chassis.CQRS.Command;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using BookWorm.Ordering.Infrastructure.Helpers;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderCommand : ICommand<Guid>
{
    [JsonIgnore]
    public List<OrderItem> Items { get; set; } = [];
}

public sealed class CreateOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal,
    IDistributedAccessLockProvider lockProvider
) : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

        var order = new Order(userId, null, request.Items);

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
            }
            else
            {
                throw new InvalidOperationException("Other process is already creating an order");
            }
        }

        return result.Id;
    }
}
