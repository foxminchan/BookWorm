using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.EventBus.Dispatcher;

public interface IEventMapper
{
    IntegrationEvent MapToIntegrationEvent(DomainEvent @event);
}
