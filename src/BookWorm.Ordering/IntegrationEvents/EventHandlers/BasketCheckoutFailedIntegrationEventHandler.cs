using Ardalis.GuardClauses;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.Events;
using MassTransit;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

public sealed class BasketCheckoutFailedIntegrationEventHandler(IRepository<Order> repository)
    : IConsumer<BasketCheckoutFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutFailedIntegrationEvent> context)
    {
        var order = await repository.GetByIdAsync(context.Message.OrderId);

        Guard.Against.NotFound(context.Message.OrderId, order);

        await repository.DeleteAsync(order);
    }
}
