using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Chat.Memory;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.Features.Memories;

internal sealed class DeleteUserMemoryEndpoint
    : IEndpoint<NoContent, IUserIdentityAccessor, IUserMemoryService>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/memories",
                async (
                    IUserIdentityAccessor identityAccessor,
                    IUserMemoryService memoryService,
                    CancellationToken cancellationToken
                ) => await HandleAsync(identityAccessor, memoryService, cancellationToken)
            )
            .ProducesDelete()
            .WithTags(UserMemoryEndpoints.Tag)
            .WithName(nameof(DeleteUserMemoryEndpoint))
            .WithSummary("Delete My Memory")
            .WithDescription(
                "Delete every preference and past interaction BookAgent remembers about the current user"
            )
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        IUserIdentityAccessor identityAccessor,
        IUserMemoryService memoryService,
        CancellationToken cancellationToken = default
    )
    {
        var userId = Guard.Against.NotAuthenticated(identityAccessor.GetUserId());

        await memoryService.ForgetAllAsync(userId, cancellationToken);

        return TypedResults.NoContent();
    }
}
