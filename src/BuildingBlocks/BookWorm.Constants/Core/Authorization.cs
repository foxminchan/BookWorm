namespace BookWorm.Constants.Core;

public static class Authorization
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string User = "user";
        public const string Reporter = "reporter";
    }

    public static class Policies
    {
        public const string Admin = Roles.Admin;
        public const string User = Roles.User;
        public const string Reporter = Roles.Reporter;
    }

    public static class Actions
    {
        public const string Read = "read";
        public const string Write = "write";
    }
}
