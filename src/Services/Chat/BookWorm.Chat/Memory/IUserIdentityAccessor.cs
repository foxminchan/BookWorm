using System.Security.Claims;
using BookWorm.Chassis.Security.Extensions;

namespace BookWorm.Chat.Memory;

/// <summary>
///     Resolves the authenticated user for the current agent invocation.
/// </summary>
internal interface IUserIdentityAccessor
{
    /// <summary>
    ///     Gets the authenticated user's stable identifier, or <see langword="null" /> when the
    ///     current invocation is anonymous or has no subject claim.
    /// </summary>
    string? GetUserId();
}

/// <summary>
///     Resolves the user from the ambient <see cref="HttpContext" /> on every call.
/// </summary>
/// <remarks>
///     Agents are singletons that allow concurrent invocations, so the user must be resolved per
///     invocation rather than captured at construction time. <see cref="IHttpContextAccessor" /> flows
///     with the async execution context of the AG-UI and A2A requests that drive the agent.
/// </remarks>
internal sealed class HttpUserIdentityAccessor(IHttpContextAccessor httpContextAccessor)
    : IUserIdentityAccessor
{
    public string? GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var userId = user.GetClaimValue(ClaimTypes.NameIdentifier);

        return string.IsNullOrWhiteSpace(userId) ? null : userId;
    }
}
