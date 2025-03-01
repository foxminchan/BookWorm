namespace BookWorm.Ordering.Helpers;

public static class BuyerIdHelper
{
    public static Guid ToBuyerId(this string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        if (!Guid.TryParse(userId, out var buyerId))
        {
            throw new ArgumentException("Invalid Buyer ID format.", nameof(userId));
        }

        return buyerId;
    }
}
