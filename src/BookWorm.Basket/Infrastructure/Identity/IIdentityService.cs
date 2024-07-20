namespace BookWorm.Basket.Infrastructure.Identity;

public interface IIdentityService
{
    string? GetUserIdentity();

    string? GetUserName();
}
