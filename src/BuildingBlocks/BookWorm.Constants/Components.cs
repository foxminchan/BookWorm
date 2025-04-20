namespace BookWorm.Constants;

public static class Components
{
    public const string Redis = "redis";
    public const string Postgres = "postgres";
    public const string VectorDb = "vectordb";
    public const string KeyCloak = "keycloak";
    public const string MailPit = "mailpit";
    public const string Queue = "queue";

    public static class Database
    {
        public const string Catalog = "catalogdb";
        public const string Ordering = "orderingdb";
        public const string Rating = "ratingdb";
        public const string Finance = "financedb";
    }

    public static class Ollama
    {
        public const string Resource = "ollama";
        public const string Chat = "chat";
        public const string Embedding = "embedding";
    }

    public static class Azure
    {
        public static class Storage
        {
            public const string Resource = "storage";
            public const string Blob = "blob";
            public const string Table = "table";
        }

        public const string SignalR = "signalr";
    }
}
