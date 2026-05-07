using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace BookWorm.Chassis.EventBus.Wolverine;

/// <summary>
///     Wolverine envelope rule that stamps the current HTTP user identifier onto
///     outbound <see cref="Envelope.Headers" /> so it surfaces as the CloudEvent
///     <c>userid</c> extension attribute (FR-009). Replaces the MassTransit
///     <c>UserIdPublishFilter</c>.
/// </summary>
internal sealed class UserIdEnvelopeMiddleware(IHttpContextAccessor contextAccessor) : IEnvelopeRule
{
    /// <summary>
    ///     Copies the authenticated user's name identifier into the envelope's
    ///     <see cref="EventBusHeaders.UserId" /> header when an
    ///     <see cref="HttpContext" /> is available. The CloudEvents mapper then
    ///     includes it as the <c>userid</c> CE extension attribute.
    /// </summary>
    public void Modify(Envelope envelope)
    {
        var userId = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            envelope.Headers.TryAdd(EventBusHeaders.UserId, userId);
            // Also stamp the CloudEvent extension attribute name directly so the
            // built-in mapper does not need to rename the header.
            envelope.Headers.TryAdd("userid", userId);
        }
    }

    /// <summary>
    ///     When publishing within a handler context, fall back to propagating
    ///     the user ID from the originating envelope header if the HTTP context
    ///     is no longer available (e.g. background processing).
    /// </summary>
    public void ApplyCorrelation(IMessageContext originator, Envelope outgoing)
    {
        // Try HTTP context first (takes priority).
        Modify(outgoing);

        // If HTTP context was unavailable, fall back to the incoming envelope header.
        if (
            !outgoing.Headers.ContainsKey(EventBusHeaders.UserId)
            && originator.Envelope?.Headers.TryGetValue(EventBusHeaders.UserId, out var userId)
                == true
            && !string.IsNullOrEmpty(userId)
        )
        {
            outgoing.Headers.TryAdd(EventBusHeaders.UserId, userId);
            outgoing.Headers.TryAdd("userid", userId);
        }
    }
}
