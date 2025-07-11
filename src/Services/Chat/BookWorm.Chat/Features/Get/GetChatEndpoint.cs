﻿using System.ComponentModel;

namespace BookWorm.Chat.Features.Get;

public sealed class GetChatEndpoint : IEndpoint<Ok<ConversationDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{id:guid}",
                async (
                    [Description("The unique identifier of the chat to be retrieved")] Guid id,
                    ISender sender,
                    CancellationToken cancellationToken
                ) => await HandleAsync(id, sender, cancellationToken)
            )
            .ProducesGet<ConversationDto>(hasNotFound: true)
            .WithTags(nameof(Chat))
            .WithName(nameof(GetChatEndpoint))
            .WithSummary("Get Chat")
            .WithDescription("Get a chat by its ID")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<ConversationDto>> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var conversation = await sender.Send(new GetChatQuery(id), cancellationToken);

        return TypedResults.Ok(conversation);
    }
}
