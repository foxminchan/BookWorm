namespace BookWorm.Constants;

public static class Authorization
{
    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string User = "USER";
    }

    public static class Policies
    {
        public const string Admin = nameof(Admin);
        public const string User = nameof(User);
    }
}
