using BookWorm.Chat.Domain.AggregatesModel;

namespace BookWorm.Chat.Features.Get;

public sealed record GetChatQuery(Guid Id) : IQuery<ConversationDto>;

public sealed class GetChatHandler(IConversationRepository repository)
    : IQueryHandler<GetChatQuery, ConversationDto>
{
    public async Task<ConversationDto> Handle(
        GetChatQuery request,
        CancellationToken cancellationToken
    )
    {
        var conversation = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(conversation, request.Id);

        return conversation.ToConversationDto();
    }
}
