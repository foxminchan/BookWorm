using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookWorm.Shared.Exceptions;

public sealed class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        logger.LogError(notFoundException, "[{Handler}] Exception occurred: {Message}",
            nameof(NotFoundExceptionHandler), notFoundException.Message);

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status404NotFound,
            Type = notFoundException.GetType().Name,
            Title = "Entity was not found",
            Detail = notFoundException.Message
        };

        await TypedResults.NotFound(problemDetails).ExecuteAsync(httpContext);

        return true;
    }
}
