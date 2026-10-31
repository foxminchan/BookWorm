using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Chat.Memory;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.Features.Memories;

internal sealed class GetUserMemoryEndpoint
    : IEndpoint<Ok<UserMemoryDto>, IUserIdentityAccessor, IUserMemoryService>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/memories",
                async (
                    IUserIdentityAccessor identityAccessor,
                    IUserMemoryService memoryService,
                    CancellationToken cancellationToken
                ) => await HandleAsync(identityAccessor, memoryService, cancellationToken)
            )
            .ProducesGet<UserMemoryDto>()
            .WithTags(UserMemoryEndpoints.Tag)
            .WithName(nameof(GetUserMemoryEndpoint))
            .WithSummary("Get My Memory")
            .WithDescription(
                "Get the preferences and past interactions BookAgent remembers about the current user"
            )
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<UserMemoryDto>> HandleAsync(
        IUserIdentityAccessor identityAccessor,
        IUserMemoryService memoryService,
        CancellationToken cancellationToken = default
    )
    {
        var userId = Guard.Against.NotAuthenticated(identityAccessor.GetUserId());

        var snapshot = await memoryService.GetAllAsync(userId, cancellationToken);

        return TypedResults.Ok(snapshot.ToDto());
    }
}
