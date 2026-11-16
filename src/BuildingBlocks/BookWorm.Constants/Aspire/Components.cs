namespace BookWorm.Constants.Aspire;

public static class Components
{
    public const string K6 = "k6";
    public const string Redis = "redis";
    public const string Broker = "broker";
    public const string MailPit = "mailpit";
    public const string Email = "email";
    public const string Postgres = "postgres";
    public const string Cosmos = "cosmos";
    public const string Inspector = "inspector";
    public const string VectorDb = "vectordb";
    public const string TurboRepo = "turborepo";

    public static class Database
    {
        public const string Rating = "ratingdb";
        public const string Catalog = "catalogdb";
        public const string Ordering = "orderingdb";
        public const string Scheduler = "schedulerdb";
    }

    public static class Container
    {
        public const string Feedbacks = "feedbacks";
    }

    public static class Foundry
    {
        public const string Resource = nameof(Foundry);
        public const string Chat = "chat";
        public const string Decision = "decision";
        public const string Embedding = "embedding";
        public const string Project = $"{Resource}-project";
    }

    public static class Presidio
    {
        public const string Analyzer = "presidio-analyzer";
        public const string Anonymizer = "presidio-anonymizer";
    }

    public static class Azure
    {
        public const string ContainerApp = "aca";
        public const string EntraId = "entra";

        public static class DurableTask
        {
            public const string Scheduler = "durable-task-scheduler";
            public const string TaskHub = "finance-taskhub";
        }

        public static class Storage
        {
            public const string Resource = "storage";

            public static string BlobContainer(string containerName)
            {
                return $"{containerName}-blob";
            }
        }
    }
}
