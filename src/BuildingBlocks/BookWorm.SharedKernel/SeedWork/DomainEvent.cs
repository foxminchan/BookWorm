using MediatR;

namespace BookWorm.SharedKernel.SeedWork;

public abstract class DomainEvent : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}
