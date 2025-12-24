namespace BookWorm.Constants.Aspire;

public static class Components
{
    public const string K6 = "k6";
    public const string Queue = "queue";
    public const string Redis = "redis";
    public const string MailPit = "mailpit";
    public const string Postgres = "postgres";
    public const string KeyCloak = "keycloak";
    public const string Inspector = "inspector";
    public const string VectorDb = "vectordb";
    public const string ContainerRegistry = "container-registry";

    public static class Database
    {
        public const string Rating = "ratingdb";
        public const string Finance = "financedb";
        public const string Catalog = "catalogdb";
        public const string Ordering = "orderingdb";
        public const string Notification = "notificationdb";
    }

    public static class OpenAI
    {
        public const string OpenAIGpt4oMini = "gpt-4o-mini";
        public const string TextEmbedding3Large = "text-embedding-3-large";
        public const string Resource = "openai";
        public const string Chat = "chat";
        public const string Embedding = "embedding";
    }

    public static class Azure
    {
        public const string ContainerApp = "aca";

        public static class Storage
        {
            public const string Resource = "storage";

            public static string BlobContainer(string containerName) => $"{containerName}-blob";
        }
    }
}
