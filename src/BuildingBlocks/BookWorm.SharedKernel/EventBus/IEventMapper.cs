using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.SharedKernel.EventBus;

public interface IEventMapper
{
    IntegrationEvent MapToIntegrationEvent(DomainEvent @event);
}
