using BookWorm.Chassis.Specification;

namespace BookWorm.Chat.Domain.AggregatesModel;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation> AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default
    );

    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> ListAsync(
        ISpecification<Conversation> spec,
        CancellationToken cancellationToken = default
    );

    bool Delete(Conversation conversation, CancellationToken cancellationToken = default);
}
