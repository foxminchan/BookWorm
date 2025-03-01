namespace BookWorm.Catalog.Features.Chat.Cancel;

public sealed class CancelChatEndpoint : IEndpoint<NoContent, Guid, ICancellationManager>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/chats/{id:guid}/cancel",
                async (Guid id, ICancellationManager manager) => await HandleAsync(id, manager)
            )
            .Produces(StatusCodes.Status204NoContent)
            .WithOpenApi()
            .WithTags(nameof(Chat))
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
