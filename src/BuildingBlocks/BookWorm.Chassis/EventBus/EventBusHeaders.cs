namespace BookWorm.Chassis.EventBus;

/// <summary>
///     Defines well-known message header keys used across the event bus infrastructure.
/// </summary>
public static class EventBusHeaders
{
    /// <summary>
    ///     The header key used to propagate the authenticated user's identifier through the event bus.
    ///     Consumers can read this header via <c>context.Headers.Get&lt;string&gt;(EventBusHeaders.UserId)</c>.
    /// </summary>
    public const string UserId = "x-user-id";
}
