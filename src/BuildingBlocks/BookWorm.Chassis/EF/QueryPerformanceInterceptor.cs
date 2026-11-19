using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.EF;

internal sealed class QueryPerformanceInterceptor(ILogger<QueryPerformanceInterceptor> logger)
    : DbCommandInterceptor
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

        logger.LogWarning(
            "Slow query detected: {CommandText} | Elapsed time: {ElapsedMilliseconds} ms",
            commandText,
            elapsedMilliseconds
        );

        return interceptionResult;
    }
}
