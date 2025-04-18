using MassTransit;

namespace BookWorm.SharedKernel.EventBus;

[ExcludeFromTopology]
public abstract record IntegrationEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public DateTime CreationDate { get; } = DateTime.UtcNow;
}
