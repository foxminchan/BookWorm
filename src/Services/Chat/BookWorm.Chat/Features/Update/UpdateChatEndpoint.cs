namespace BookWorm.Chat.Features.Update;

public sealed class UpdateChatEndpoint : IEndpoint<NoContent, UpdateChatCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/chats",
                async (UpdateChatCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPut()
            .WithTags(nameof(Chat))
            .WithName(nameof(UpdateChatEndpoint))
            .WithSummary("Update Chat")
            .WithDescription("Update an existing chat in the catalog system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<NoContent> HandleAsync(
        UpdateChatCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
