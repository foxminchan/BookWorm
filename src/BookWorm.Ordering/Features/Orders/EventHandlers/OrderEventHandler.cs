using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Domain.OrderAggregate.Events;
using Marten;
using MediatR;

namespace BookWorm.Ordering.Features.Orders.EventHandlers;

public sealed class OrderEventHandler(IDocumentSession documentSession) : INotificationHandler<OrderCreatedEvent>,
    INotificationHandler<OrderCompletedEvent>, INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        await documentSession.Events.WriteToAggregate<OrderState>(Guid.NewGuid(),
            stream => { stream.AppendOne(notification); }, cancellationToken);
    }

    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        await documentSession.Events.WriteToAggregate<OrderState>(Guid.NewGuid(),
            stream => { stream.AppendOne(notification); }, cancellationToken);
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        await documentSession.Events.WriteToAggregate<OrderState>(Guid.NewGuid(),
            stream => { stream.AppendOne(notification); }, cancellationToken);
    }
}
