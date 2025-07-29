using BookWorm.Chassis.OpenTelemetry.ActivityScope;

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

    public static class EventHandling
    {
        public const string Event = $"{ActivitySourceProvider.DefaultSourceName}.event";
    }

    public static class Validator
    {
        public const string Validation = $"{ActivitySourceProvider.DefaultSourceName}.validator";
    }
}
