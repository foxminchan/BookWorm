using Wolverine;

namespace BookWorm.Finance.Saga;

/// <summary>
///     Wolverine timeout message that replaces the MassTransit
///     <c>Schedule&lt;PlaceOrderTimeoutIntegrationEvent&gt;</c> mechanism.
///     Deferred for <see cref="OrderStateMachineSettings.MaxRetryTimeout" />.
/// </summary>
internal sealed record PlaceOrderTimeout(Guid OrderId, TimeSpan Delay) : TimeoutMessage(Delay);
