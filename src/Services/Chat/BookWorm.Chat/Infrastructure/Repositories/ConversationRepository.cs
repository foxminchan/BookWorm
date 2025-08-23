using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Evaluators;
using BookWorm.Chat.Domain.AggregatesModel;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chat.Infrastructure.Repositories;

public sealed class ConversationRepository(ChatDbContext context) : IConversationRepository
{
    private readonly ChatDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;
    public IUnitOfWork UnitOfWork => _context;

    public async Task<Conversation> AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default
    )
    {
        var entry = await _context.Conversations.AddAsync(conversation, cancellationToken);
        return entry.Entity;
    }

    public async Task<Conversation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Conversations.Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> ListAsync(
        ISpecification<Conversation> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(_context.Conversations, spec)
            .ToListAsync(cancellationToken);
    }

    public bool Delete(Conversation conversation)
    {
        _context.Conversations.Remove(conversation);
        return true;
    }
}
