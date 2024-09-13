using EntityFramework.Exceptions.Common;

namespace BookWorm.Shared.Exceptions;

public sealed class UniqueConstraintExceptionHandler(ILogger<UniqueConstraintExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UniqueConstraintException uniqueConstraintException)
        {
            return false;
        }

        logger.LogError(uniqueConstraintException, "[{Handler}] Exception occurred: {Message}",
            nameof(UniqueConstraintExceptionHandler), uniqueConstraintException.Message);

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status429TooManyRequests,
            Type = uniqueConstraintException.GetType().Name,
            Title = "Duplicate unique constraint violation",
            Detail = "There was a conflict with the unique constraint",
            Extensions =
            {
                ["errors"] = new ConstraintViolation(uniqueConstraintException.ConstraintName,
                    uniqueConstraintException.Message)
            }
        };

        await TypedResults.Conflict(problemDetails).ExecuteAsync(httpContext);

        return true;
    }
}

internal record ConstraintViolation(string ConstraintName, string Message);
