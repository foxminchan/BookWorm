namespace BookWorm.Constants.Aspire;

public static class Components
{
    public static readonly string K6 = nameof(K6).ToLowerInvariant();
    public static readonly string Queue = nameof(Queue).ToLowerInvariant();
    public static readonly string Redis = nameof(Redis).ToLowerInvariant();
    public static readonly string MailPit = nameof(MailPit).ToLowerInvariant();
    public static readonly string Postgres = nameof(Postgres).ToLowerInvariant();
    public static readonly string VectorDb = nameof(VectorDb).ToLowerInvariant();
    public static readonly string KeyCloak = nameof(KeyCloak).ToLowerInvariant();
    public static readonly string McpInspector = nameof(McpInspector).ToLowerInvariant();

    public static class Database
    {
        private const string Suffix = "db";
        public static readonly string Catalog = $"{nameof(Catalog).ToLowerInvariant()}{Suffix}";
        public static readonly string Ordering = $"{nameof(Ordering).ToLowerInvariant()}{Suffix}";
        public static readonly string Rating = $"{nameof(Rating).ToLowerInvariant()}{Suffix}";
        public static readonly string Finance = $"{nameof(Finance).ToLowerInvariant()}{Suffix}";
        public static readonly string Chat = $"{nameof(Chat).ToLowerInvariant()}{Suffix}";
        public static readonly string User = $"{nameof(User).ToLowerInvariant()}{Suffix}";
        public static readonly string Health = $"{nameof(Health).ToLowerInvariant()}{Suffix}";
    }

    public static class Ollama
    {
        public static readonly string Resource = nameof(Ollama).ToLowerInvariant();
        public static readonly string Chat = $"{nameof(Chat).ToLowerInvariant()}";
        public static readonly string Embedding = $"{nameof(Embedding).ToLowerInvariant()}";
    }

    public static class Azure
    {
        public const string ContainerApp = "aca";
        public static readonly string SignalR = nameof(SignalR).ToLowerInvariant();
        public static readonly string ApplicationInsights = nameof(ApplicationInsights)
            .ToLowerInvariant();

        public static class Storage
        {
            public static readonly string Resource = nameof(Storage).ToLowerInvariant();
            public static readonly string Blob = nameof(Blob).ToLowerInvariant();
            public static readonly string Table = nameof(Table).ToLowerInvariant();
            public static readonly string BlobContainer = $"{Services.Catalog}-{Blob}";
        }
    }
}
