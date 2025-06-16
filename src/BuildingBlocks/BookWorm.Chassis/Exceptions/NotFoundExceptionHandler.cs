using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException For<T>(Guid id)
    {
        return For<T>(id.ToString());
    }

    public static NotFoundException For<T>(string id)
    {
        return new($"{typeof(T).Name} with id {id} not found.");
    }
}

public sealed class NotFoundExceptionHandler(
    ILogger<NotFoundExceptionHandler> logger,
    PerRequestLogBuffer logBuffer
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        logger.LogWarning(
            exception,
            "[{Handler}] Not found exception occurred: {Message}",
            nameof(NotFoundExceptionHandler),
            notFoundException.Message
        );

        logBuffer.Flush();

        await TypedResults
            .NotFound(new { Detail = notFoundException.Message })
            .ExecuteAsync(httpContext);

        return true;
    }
}
