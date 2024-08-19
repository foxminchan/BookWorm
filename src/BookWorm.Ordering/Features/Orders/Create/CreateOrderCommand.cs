using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.IntegrationEvents.Events;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed record CreateOrderCommand(string? Note) : ICommand<Result<Guid>>;

public sealed class CreateOrderHandler(
    IRepository<Order> repository,
    IBasketService basketService,
    IIdentityService identityService,
    IPublishEndpoint publishEndpoint) : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var buyerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(buyerId);

        var basket = await basketService.GetBasketAsync(Guid.Parse(buyerId), cancellationToken);

        Guard.Against.Null(basket);

        var order = new Order(Guid.Parse(buyerId), request.Note);

        foreach (var item in basket.Items)
        {
            order.AddOrderItem(item.BookId, item.Price, item.Quantity);
        }

        var result = await repository.AddAsync(order, cancellationToken);

        var email = identityService.GetEmail();

        var @event = new OrderCreatedIntegrationEvent(result.Id, basket.Id, email);

        await publishEndpoint.Publish(@event, cancellationToken);

        return result.Id;
    }
}
