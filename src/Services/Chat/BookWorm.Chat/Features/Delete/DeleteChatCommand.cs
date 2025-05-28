﻿using BookWorm.Chat.Domain.AggregatesModel;

namespace BookWorm.Chat.Features.Delete;

public sealed record DeleteChatCommand(Guid Id) : ICommand;

public sealed class DeleteChatHandler(IConversationRepository repository)
    : ICommandHandler<DeleteChatCommand>
{
    public async Task<Unit> Handle(DeleteChatCommand request, CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(conversation, $"Conversation with id {request.Id} not found.");

        repository.Delete(conversation, cancellationToken);

        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
