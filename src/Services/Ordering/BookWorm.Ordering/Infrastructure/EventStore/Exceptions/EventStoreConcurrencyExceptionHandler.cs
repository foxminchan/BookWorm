using JasperFx.Events;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Ordering.Infrastructure.EventStore.Exceptions;

internal sealed class EventStoreConcurrencyExceptionHandler(
    ILogger<EventStoreConcurrencyExceptionHandler> logger,
    PerRequestLogBuffer logBuffer
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var problemDetails = exception switch
        {
            ConcurrencyException => new ProblemDetails
            {
                Status = StatusCodes.Status412PreconditionFailed,
                Title = "Precondition Failed",
                Detail = "The resource was modified concurrently. Refresh and retry.",
            },
            ExistingStreamIdCollisionException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "An event stream with the specified identifier already exists.",
            },
            _ => null,
        };

        if (problemDetails is null)
        {
            return false;
        }

        logger.LogWarning(
            exception,
            "[{Handler}] Event store concurrency failure ({Status}): {Message}",
            nameof(EventStoreConcurrencyExceptionHandler),
            problemDetails.Status,
            exception.Message
        );

        logBuffer.Flush();

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            EventStoreExceptionSerializationContext.Default.ProblemDetails,
            cancellationToken: cancellationToken
        );

        return true;
    }
}

[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class EventStoreExceptionSerializationContext : JsonSerializerContext;
