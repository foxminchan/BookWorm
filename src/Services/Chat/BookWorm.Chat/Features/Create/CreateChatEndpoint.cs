using BookWorm.Chat.Features.Stream;

namespace BookWorm.Chat.Features.Create;

public sealed class CreateChatEndpoint
    : IEndpoint<Created<Guid>, CreateChatCommand, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                string.Empty,
                async (CreateChatCommand request, ISender sender, LinkGenerator linker) =>
                    await HandleAsync(request, sender, linker)
            )
            .ProducesPost<Guid>()
            .WithTags(nameof(Chat))
            .WithName(nameof(CreateChatEndpoint))
            .WithSummary("Create Chat")
            .WithDescription("Create a new chat session in the chat system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateChatCommand command,
        ISender sender,
        LinkGenerator linker,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        var path = linker.GetPathByName(nameof(ChatStreamEndpoint), new { id = result });

        return TypedResults.Created(path, result);
    }
}
