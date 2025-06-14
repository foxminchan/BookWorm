using BookWorm.Chassis.ActivityScope;

namespace BookWorm.Chassis.OpenTelemetry;

public static class TelemetryTags
{
    public static class Commands
    {
        public const string Command = $"{ActivitySourceProvider.DefaultSourceName}.command";
        public const string CommandType = $"{Command}.type";
        private const string CommandsMeter = $"{ActivitySourceProvider.DefaultSourceName}.commands";
        private const string CommandHandling = $"{CommandsMeter}.handling";
        public const string ActiveCommandsNumber = $"{CommandHandling}.active.number";
        public const string TotalCommandsNumber = $"{CommandHandling}.total";
        public const string CommandHandlingDuration = $"{CommandHandling}.duration";
    }

    public static class Queries
    {
        public const string Query = $"{ActivitySourceProvider.DefaultSourceName}.query";
        public const string QueryType = $"{Query}.type";
        private const string QueriesMeter = $"{ActivitySourceProvider.DefaultSourceName}.queries";
        private const string QueryHandling = $"{QueriesMeter}.handling";
        public const string ActiveQueriesNumber = $"{QueryHandling}.active.number";
        public const string TotalQueriesNumber = $"{QueryHandling}.total";
        public const string QueryHandlingDuration = $"{QueryHandling}.duration";
    }

    public static class Validator
    {
        public const string Validation = $"{ActivitySourceProvider.DefaultSourceName}.validator";
    }

    public static class Messaging
    {
        public const string System = "messaging.system";
        public const string Operation = "messaging.operation";
        public const string MessageId = "messaging.message_id";
        public const string ConversationId = "messaging.conversation_id";
        public const string CorrelationId = "messaging.correlation_id";
        public const string MessageType = "messaging.message_type";
        public const string Destination = "messaging.destination";
        public const string SourceAddress = "messaging.source_address";
        public const string RequestId = "messaging.request_id";
        public const string Consumer = "messaging.consumer";
        public const string SentTime = "messaging.sent_time";
        public const string TimeToLive = "messaging.ttl";
        public const string Delay = "messaging.delay";
    }
}
