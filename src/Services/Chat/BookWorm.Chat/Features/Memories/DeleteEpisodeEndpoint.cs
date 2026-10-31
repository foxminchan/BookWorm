using System.ComponentModel;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Chat.Memory;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.Features.Memories;

internal sealed class DeleteEpisodeEndpoint
    : IEndpoint<NoContent, Guid, IUserIdentityAccessor, IUserMemoryService>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/memories/episodes/{id:guid}",
                async (
                    [Description("The unique identifier of the remembered interaction")] Guid id,
                    IUserIdentityAccessor identityAccessor,
                    IUserMemoryService memoryService,
                    CancellationToken cancellationToken
                ) => await HandleAsync(id, identityAccessor, memoryService, cancellationToken)
            )
            .ProducesDelete()
            .WithTags(UserMemoryEndpoints.Tag)
            .WithName(nameof(DeleteEpisodeEndpoint))
            .WithSummary("Delete Remembered Interaction")
            .WithDescription(
                "Delete a single past interaction BookAgent remembers about the current user"
            )
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        IUserIdentityAccessor identityAccessor,
        IUserMemoryService memoryService,
        CancellationToken cancellationToken = default
    )
    {
        var userId = Guard.Against.NotAuthenticated(identityAccessor.GetUserId());

        if (!await memoryService.ForgetEpisodeAsync(userId, id, cancellationToken))
        {
            throw NotFoundException.For<EpisodeDto>(id);
        }

        return TypedResults.NoContent();
    }
}
