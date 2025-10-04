using BookWorm.Chassis.CQRS.Command;

namespace BookWorm.Chat.Features.Create;

public sealed record CreateChatCommand(Prompt Prompt) : ICommand;

public sealed class UpdateChatHandler(IChatStreaming chatStreaming)
    : ICommandHandler<CreateChatCommand>
{
    public async Task<Unit> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        await chatStreaming.AddStreamingMessage(Guid.CreateVersion7(), request.Prompt.Text);

        return Unit.Value;
    }
}
