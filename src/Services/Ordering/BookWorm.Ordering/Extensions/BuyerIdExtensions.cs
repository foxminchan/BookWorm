using BookWorm.Chassis.Utilities.Guards;

namespace BookWorm.Ordering.Extensions;

internal static class BuyerIdExtensions
{
    extension(string? userId)
    {
        public BuyerId ToBuyerId()
        {
            userId = Guard.Against.NotAuthenticated(userId);

            return !Guid.TryParse(userId, out var buyerId)
                ? throw new ArgumentException("Invalid Buyer ID format.", nameof(userId))
                : BuyerId.From(buyerId);
        }
    }
}
