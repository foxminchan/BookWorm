using System.ComponentModel;

namespace BookWorm.Chat.Features.Cancel;

public sealed class CancelChatEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/chats/{id:guid}/cancel",
                async (
                    [Description("The unique identifier of the chat to be cancelled")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Chat))
            .WithName(nameof(CancelChatEndpoint))
            .WithSummary("Cancel Chat")
            .WithDescription("Cancel a chat if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new CancelChatCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
