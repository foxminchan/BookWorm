namespace BookWorm.Chat.Features.Update;

public sealed record UpdateChatCommand(Guid Id, Prompt Prompt) : ICommand;

public sealed class UpdateChatHandler(IChatStreaming chatStreaming)
    : ICommandHandler<UpdateChatCommand>
{
    public async Task<Unit> Handle(UpdateChatCommand request, CancellationToken cancellationToken)
    {
        await chatStreaming.AddStreamingMessage(request.Id, request.Prompt.Text);

        return Unit.Value;
    }
}
