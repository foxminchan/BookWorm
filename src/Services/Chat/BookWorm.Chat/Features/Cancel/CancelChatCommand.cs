namespace BookWorm.Chat.Features.Cancel;

public sealed record CancelChatCommand(Guid Id) : ICommand;

public sealed class CancelChatHandler(ICancellationManager cancellationManager)
    : ICommandHandler<CancelChatCommand>
{
    public async Task<Unit> Handle(CancelChatCommand request, CancellationToken cancellationToken)
    {
        await cancellationManager.CancelAsync(request.Id);

        return Unit.Value;
    }
}
