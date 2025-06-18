namespace BookWorm.Chat.Features.Delete;

public sealed class DeleteChatEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Chat))
            .WithName(nameof(DeleteChatEndpoint))
            .WithSummary("Delete Chat")
            .WithDescription("Delete a chat by its ID")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteChatCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
