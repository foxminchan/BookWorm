using System.ComponentModel;

namespace BookWorm.Chat.Features.Cancel;

public sealed class CancelChatEndpoint : IEndpoint<NoContent, Guid, ICancellationManager>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/chats/{id:guid}/cancel",
                async (
                    [Description("The unique identifier of the chat to be cancelled")] Guid id,
                    ICancellationManager manager
                ) => await HandleAsync(id, manager)
            )
            .ProducesDelete()
            .WithTags(nameof(Chat))
            .WithName(nameof(CancelChatEndpoint))
            .WithSummary("Cancel Chat")
            .WithDescription("Cancel a chat if it exists")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ICancellationManager manager,
        CancellationToken cancellationToken = default
    )
    {
        await manager.CancelAsync(id);

        return TypedResults.NoContent();
    }
}
