using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Contracts;

public abstract record IntegrationEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public DateTime CreationDate { get; } = DateTimeHelper.UtcNow();
}
