using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Chat.Features.Create;

public sealed record CreateChatCommand(Prompt Prompt) : ICommand<Guid>;

public sealed class UpdateChatHandler(IChatStreaming chatStreaming)
    : ICommandHandler<CreateChatCommand, Guid>
{
    public async Task<Guid> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        var conversationId = Guid.CreateVersion7();

        await chatStreaming.AddStreamingMessage(conversationId, request.Prompt.Text);

        return conversationId;
    }
}
