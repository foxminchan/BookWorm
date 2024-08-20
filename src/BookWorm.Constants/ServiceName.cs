namespace BookWorm.Constants;

public static class ServiceName
{
    public const string EventBus = "eventbus";

    public const string Redis = "redis";

    public const string OpenAi = "openai";

    public const string Blob = "blob";

    public const string Mail = "mailserver";

    public static class Database
    {
        public const string Catalog = "catalogdb";
        public const string Ordering = "orderingdb";
        public const string Identity = "identitydb";
        public const string Notification = "notificationdb";
        public const string Rating = "ratingdb";
    }
}
