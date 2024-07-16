using BookWorm.Shared.ActivityScope;

namespace BookWorm.Shared.OpenTelemetry;

public static class TelemetryTags
{
    public static class Commands
    {
        public const string Command = $"{ActivitySourceProvider.DefaultSourceName}.command";
        public const string CommandType = $"{Command}.type";
        public const string CommandsMeter = $"{ActivitySourceProvider.DefaultSourceName}.commands";
        public const string CommandHandling = $"{CommandsMeter}.handling";
        public const string ActiveCommandsNumber = $"{CommandHandling}.active.number";
        public const string TotalCommandsNumber = $"{CommandHandling}.total";
        public const string CommandHandlingDuration = $"{CommandHandling}.duration";
    }

    public static class Queries
    {
        public const string Query = $"{ActivitySourceProvider.DefaultSourceName}.query";
        public const string QueryType = $"{Query}.type";
        public const string QueriesMeter = $"{ActivitySourceProvider.DefaultSourceName}.queries";
        public const string QueryHandling = $"{QueriesMeter}.handling";
        public const string ActiveQueriesNumber = $"{QueryHandling}.active.number";
        public const string TotalQueriesNumber = $"{QueryHandling}.total";
        public const string QueryHandlingDuration = $"{QueryHandling}.duration";
    }

    public static class Validator
    {
        public const string Validation = $"{ActivitySourceProvider.DefaultSourceName}.validator";
        public const string ValidationResponse = $"{Validation}.response";
        public const string ValidationResponseType = $"{Validation}.response.type";
        public const string ValidationRequest = $"{Validation}.request";
        public const string ValidationValidators = $"{Validation}.validators";
    }
}
