namespace BookWorm.Chat.Infrastructure.Helpers;

public static class UserIdHelper
{
    public static Guid ToUserId(this string? userId)
    {
        userId = Guard.Against.NotAuthenticated(userId);

        if (!Guid.TryParse(userId, out var buyerId))
        {
            throw new ArgumentException("Invalid User ID format.", nameof(userId));
        }

        return buyerId;
    }
}
