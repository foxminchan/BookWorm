namespace BookWorm.Constants.Core;

public static class Authorization
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string BusinessUser = "business_user";
        public const string Customer = "customer";
    }

    public static class Policies
    {
        public const string Admin = Roles.Admin;
        public const string BusinessUser = Roles.BusinessUser;
        public const string Customer = Roles.Customer;
        public const string Read = Actions.Read;
    }

    public static class Actions
    {
        public const string Read = "read";
        public const string Write = "write";
    }
}
