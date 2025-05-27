using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Infrastructure.Helpers;

namespace BookWorm.Chat.Features.Create;

public sealed record CreateChatCommand(string Name) : ICommand<Guid>;

public sealed class CreateChatHandler(
    IConversationRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CreateChatCommand, Guid>
{
    public async Task<Guid> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToUserId();

        var conversation = new Conversation(request.Name, userId);

        var result = await repository.AddAsync(conversation, cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
