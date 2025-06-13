using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.EF;

public sealed class QueryPerformanceInterceptor(ILogger<QueryPerformanceInterceptor> logger)
    : DbCommandInterceptor
{
    private const long QueryThreshold = 100;

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result
    )
    {
        var stopwatch = Stopwatch.StartNew();

        var interceptionResult = base.ReaderExecuting(command, eventData, result);

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds <= QueryThreshold)
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
            stopwatch.ElapsedMilliseconds
        );

        return interceptionResult;
    }
}
