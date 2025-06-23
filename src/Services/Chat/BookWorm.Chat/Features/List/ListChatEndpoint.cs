namespace BookWorm.Chat.Features.List;

public sealed class ListChatEndpoint
    : IEndpoint<Ok<IReadOnlyList<ConversationDto>>, ListChatQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                string.Empty,
                async ([AsParameters] ListChatQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .ProducesGet<IReadOnlyList<ConversationDto>>(true)
            .WithTags(nameof(Chat))
            .WithName(nameof(ListChatEndpoint))
            .WithSummary("List Chats")
            .WithDescription("List all chats")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<IReadOnlyList<ConversationDto>>> HandleAsync(
        ListChatQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var conversations = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(conversations);
    }
}
