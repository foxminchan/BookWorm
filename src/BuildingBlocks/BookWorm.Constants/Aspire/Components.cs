namespace BookWorm.Constants.Aspire;

public static class Components
{
    public static readonly string K6 = nameof(K6).ToLowerInvariant();
    public static readonly string Queue = nameof(Queue).ToLowerInvariant();
    public static readonly string Redis = nameof(Redis).ToLowerInvariant();
    public static readonly string MailPit = nameof(MailPit).ToLowerInvariant();
    public static readonly string Postgres = nameof(Postgres).ToLowerInvariant();
    public static readonly string KeyCloak = nameof(KeyCloak).ToLowerInvariant();
    public static readonly string Inspector = nameof(Inspector).ToLowerInvariant();
    public static readonly string VectorDb = nameof(VectorDb).ToLowerInvariant();

    public static class Database
    {
        private const string Suffix = "db";

        public static readonly string Chat = $"{nameof(Chat).ToLowerInvariant()}{Suffix}";
        public static readonly string User = $"{nameof(User).ToLowerInvariant()}{Suffix}";
        public static readonly string Rating = $"{nameof(Rating).ToLowerInvariant()}{Suffix}";
        public static readonly string Finance = $"{nameof(Finance).ToLowerInvariant()}{Suffix}";
        public static readonly string Catalog = $"{nameof(Catalog).ToLowerInvariant()}{Suffix}";
        public static readonly string Ordering = $"{nameof(Ordering).ToLowerInvariant()}{Suffix}";
        public static readonly string Scheduler = $"{nameof(Scheduler).ToLowerInvariant()}{Suffix}";

        public static readonly string Notification =
            $"{nameof(Notification).ToLowerInvariant()}{Suffix}";
    }

    public static class OpenAI
    {
        public const string OpenAIGpt4oMini = "gpt-4o-mini";
        public const string TextEmbedding3Large = "text-embedding-3-large";
        public static readonly string Resource = nameof(OpenAI).ToLowerInvariant();
        public static readonly string Chat = $"{nameof(Chat).ToLowerInvariant()}";
        public static readonly string Embedding = $"{nameof(Embedding).ToLowerInvariant()}";
    }

    public static class Azure
    {
        public const string ContainerApp = "aca";
        public static readonly string SignalR = nameof(SignalR).ToLowerInvariant();

        public static class Storage
        {
            public static readonly string Resource = nameof(Storage).ToLowerInvariant();
            public static readonly string Blob = nameof(Blob).ToLowerInvariant();
            public static readonly string BlobContainer = $"{Services.Catalog}-{Blob}";
        }
    }
}
