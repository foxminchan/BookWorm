namespace BookWorm.Catalog.Features.Chat.Create;

public sealed class CreateChatEndpoint : IEndpoint<Created<Guid>, Prompt, IChatStreaming>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/chats",
                async (Prompt prompt, IChatStreaming chat) => await HandleAsync(prompt, chat)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .WithOpenApi()
            .WithTags(nameof(Chat))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(
        Prompt prompt,
        IChatStreaming chat,
        CancellationToken cancellationToken = default
    )
    {
        var result = await chat.AddStreamingMessage(prompt.Text);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource(nameof(Chat)).WithId(result).Build(),
            result
        );
    }
}
