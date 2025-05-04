using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.EventBus;

public interface IEventMapper
{
    IntegrationEvent MapToIntegrationEvent(DomainEvent @event);
}
