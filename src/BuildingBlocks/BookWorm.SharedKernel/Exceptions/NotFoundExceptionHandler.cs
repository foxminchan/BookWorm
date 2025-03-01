using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookWorm.SharedKernel.Exceptions;

public class NotFoundException(string message) : Exception(message);

public sealed class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    : IExceptionHandler
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

        await TypedResults
            .NotFound(new { Detail = notFoundException.Message })
            .ExecuteAsync(httpContext);

        return true;
    }
}
