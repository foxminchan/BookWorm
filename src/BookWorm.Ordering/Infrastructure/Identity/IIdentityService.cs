namespace BookWorm.Ordering.Infrastructure.Identity;

public interface IIdentityService
{
    string? GetUserIdentity();

    string? GetUserName();
}
