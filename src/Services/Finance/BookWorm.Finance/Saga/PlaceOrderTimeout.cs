using Wolverine;

namespace BookWorm.Finance.Saga;

internal sealed record PlaceOrderTimeout(Guid OrderId, TimeSpan Delay) : TimeoutMessage(Delay);
