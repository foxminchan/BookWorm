using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Chat.Domain.AggregatesModel.Specifications;

public sealed class ConversationFilterSpec : Specification<Conversation>
{
    public ConversationFilterSpec(string? name, Guid? userId, bool includeMessages)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Query.Where(c => c.Name!.Contains(name));
        }

        if (userId.HasValue)
        {
            Query.Where(c => c.UserId == userId);
        }

        if (includeMessages)
        {
            Query.Include(c => c.Messages);
        }
    }
}
