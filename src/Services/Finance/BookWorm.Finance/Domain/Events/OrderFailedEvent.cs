using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Finance.Domain.Events;

public sealed class OrderFailedEvent(Order order) : DomainEvent
{
    public Order Order { get; } = order;
}
