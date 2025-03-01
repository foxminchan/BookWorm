using BookWorm.SharedKernel.EventBus;
using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.SharedKernel.SeedWork;

public interface IEventMapper
{
    IntegrationEvent MapToIntegrationEvent(DomainEvent @event);
}
