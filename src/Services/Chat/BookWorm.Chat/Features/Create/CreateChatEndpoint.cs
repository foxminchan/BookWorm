namespace BookWorm.Chat.Features.Create;

public sealed class CreateChatEndpoint : IEndpoint<NoContent, CreateChatCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                string.Empty,
                async (CreateChatCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPut()
            .WithTags(nameof(Chat))
            .WithName(nameof(CreateChatEndpoint))
            .WithSummary("Create Chat")
            .WithDescription("Create a new chat session in the chat system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        CreateChatCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
