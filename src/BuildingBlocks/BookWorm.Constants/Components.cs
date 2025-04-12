namespace BookWorm.Constants;

public static class Components
{
    public const string Blob = "blob";
    public const string Redis = "redis";
    public const string Postgres = "postgres";
    public const string VectorDb = "vectordb";
    public const string KeyCloak = "keycloak";
    public const string MailPit = "mailpit";
    public const string Queue = "queue";
    public const string Storage = "storage";
    public const string Cosmos = "cosmos";
    public const string SignalR = "signalr";

    public static class Database
    {
        public const string Catalog = "catalogdb";
        public const string Ordering = "orderingdb";
        public const string Rating = "ratingdb";
        public const string Finance = "financedb";
    }

    public static class Ollama
    {
        public const string Chat = "chat";
        public const string Embedding = "embedding";
    }
}
