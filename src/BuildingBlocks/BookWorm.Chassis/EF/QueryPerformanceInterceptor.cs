using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.EF;

internal sealed partial class QueryPerformanceInterceptor(
    ILogger<QueryPerformanceInterceptor> logger
) : DbCommandInterceptor
{
    private const long QueryThreshold = 100;

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result
    )
    {
        var start = Stopwatch.GetTimestamp();

        var interceptionResult = base.ReaderExecuting(command, eventData, result);

        var elapsedMilliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        if (elapsedMilliseconds <= QueryThreshold)
        {
            return interceptionResult;
        }

        var commandText = command.CommandText;
        if (command.Parameters.Count > 0)
        {
            commandText += " | Parameters: " + string.Join(", ", command.Parameters);
        }

        LogSlowQuery(logger, commandText, elapsedMilliseconds);

        return interceptionResult;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Slow query detected: {CommandText} | Elapsed time: {ElapsedMilliseconds} ms"
    )]
    private static partial void LogSlowQuery(
        ILogger logger,
        string commandText,
        double elapsedMilliseconds
    );
}
