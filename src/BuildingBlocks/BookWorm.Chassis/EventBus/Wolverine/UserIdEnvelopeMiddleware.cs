using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace BookWorm.Chassis.EventBus.Wolverine;

internal sealed class UserIdEnvelopeMiddleware(IHttpContextAccessor contextAccessor) : IEnvelopeRule
{
    public void Modify(Envelope envelope)
    {
        var userId = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        envelope.Headers.TryAdd(EventBusHeaders.UserId, userId);
        envelope.Headers.TryAdd("userid", userId);
    }

    public void ApplyCorrelation(IMessageContext originator, Envelope outgoing)
    {
        // Try HTTP context first (takes priority).
        Modify(outgoing);

        // If HTTP context was unavailable, fall back to the incoming envelope header.
        if (
            outgoing.Headers.ContainsKey(EventBusHeaders.UserId)
            || originator.Envelope?.Headers.TryGetValue(EventBusHeaders.UserId, out var userId)
                != true
            || string.IsNullOrEmpty(userId)
        )
        {
            return;
        }

        outgoing.Headers.TryAdd(EventBusHeaders.UserId, userId);
        outgoing.Headers.TryAdd("userid", userId);
    }
}
