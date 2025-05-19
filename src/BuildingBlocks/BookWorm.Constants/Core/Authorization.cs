namespace BookWorm.Constants.Core;

public static class Authorization
{
    public static class Roles
    {
        public static readonly string Admin = nameof(Admin).ToUpperInvariant();
        public static readonly string User = nameof(User).ToUpperInvariant();
    }

    public static class Policies
    {
        public const string Admin = nameof(Admin);
        public const string User = nameof(User);
    }
}
