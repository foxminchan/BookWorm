namespace BookWorm.Catalog.Features.Chat.Create;

public sealed class CreateChatEndpoint : IEndpoint<Ok<Guid>, Prompt, IChatStreaming>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/chats",
                async (Prompt prompt, IChatStreaming chat) => await HandleAsync(prompt, chat)
            )
            .Produces<Guid>()
            .WithTags(nameof(Chat))
            .WithName(nameof(CreateChatEndpoint))
            .WithSummary("Create Chat")
            .WithDescription("Create a new chat in the catalog system")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<Guid>> HandleAsync(
        Prompt prompt,
        IChatStreaming chat,
        CancellationToken cancellationToken = default
    )
    {
        var result = await chat.AddStreamingMessage(prompt.Text);

        return TypedResults.Ok(result);
    }
}
