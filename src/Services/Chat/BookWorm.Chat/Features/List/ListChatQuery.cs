using System.ComponentModel;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Domain.AggregatesModel.Specifications;
using BookWorm.Chat.Infrastructure.Helpers;

namespace BookWorm.Chat.Features.List;

public sealed record ListChatQuery(
    [property: Description("The name of the conversation to filter by.")]
    [property: DefaultValue(null)]
        string? Name = null,
    [property: Description("The user ID to filter conversations by.")]
    [property: DefaultValue(0)]
        Guid? UserId = null,
    [property: Description("Flag to include messages in the result.")]
    [property: DefaultValue(false)]
        bool IncludeMessages = false
) : IQuery<IReadOnlyList<ConversationDto>>;

public sealed class ListChatHandler(
    IConversationRepository repository,
    ClaimsPrincipal claimsPrincipal
) : IQueryHandler<ListChatQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(
        ListChatQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
        {
            request = request with
            {
                UserId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToUserId(),
            };
        }

        var spec = new ConversationFilterSpec(
            request.Name,
            request.UserId,
            request.IncludeMessages
        );

        var conversations = await repository.ListAsync(spec, cancellationToken);

        return conversations.ToConversationDtos();
    }
}
