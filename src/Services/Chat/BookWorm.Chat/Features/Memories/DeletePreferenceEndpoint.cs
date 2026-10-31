using System.ComponentModel;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Chat.Memory;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.Features.Memories;

internal sealed class DeletePreferenceEndpoint
    : IEndpoint<NoContent, Guid, IUserIdentityAccessor, IUserMemoryService>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/memories/preferences/{id:guid}",
                async (
                    [Description("The unique identifier of the remembered preference")] Guid id,
                    IUserIdentityAccessor identityAccessor,
                    IUserMemoryService memoryService,
                    CancellationToken cancellationToken
                ) => await HandleAsync(id, identityAccessor, memoryService, cancellationToken)
            )
            .ProducesDelete()
            .WithTags(UserMemoryEndpoints.Tag)
            .WithName(nameof(DeletePreferenceEndpoint))
            .WithSummary("Delete Remembered Preference")
            .WithDescription(
                "Delete a single preference BookAgent remembers about the current user"
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

        if (!await memoryService.ForgetPreferenceAsync(userId, id, cancellationToken))
        {
            throw NotFoundException.For<PreferenceDto>(id);
        }

        return TypedResults.NoContent();
    }
}
