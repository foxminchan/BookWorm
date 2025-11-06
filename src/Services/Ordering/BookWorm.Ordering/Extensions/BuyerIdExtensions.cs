using BookWorm.Chassis.Utilities.Guards;

namespace BookWorm.Ordering.Infrastructure.Helpers;

public static class BuyerIdExtensions
{
    public static Guid ToBuyerId(this string? userId)
    {
        userId = Guard.Against.NotAuthenticated(userId);

        if (!Guid.TryParse(userId, out var buyerId))
        {
            throw new ArgumentException("Invalid Buyer ID format.", nameof(userId));
        }

        return buyerId;
    }
}
