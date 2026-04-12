using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

internal sealed class NotFoundExceptionHandler(
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
            "[{Handler}] Not found: {Message}",
            nameof(NotFoundExceptionHandler),
            notFoundException.Message
        );

        logBuffer.Flush();

        await TypedResults
            .NotFound(new { Detail = "The requested resource was not found." })
            .ExecuteAsync(httpContext);

        return true;
    }
}

public static class NotFoundExceptionHandlerExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the <see cref="NotFoundExceptionHandler" /> in the ASP.NET Core exception handling pipeline.
        /// </summary>
        public void AddNotFoundExceptionHandler()
        {
            services.AddExceptionHandler<NotFoundExceptionHandler>();
        }
    }
}
