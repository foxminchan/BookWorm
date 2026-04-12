using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Exceptions;

internal sealed class ValidationExceptionHandler(
    ILogger<ValidationExceptionHandler> logger,
    PerRequestLogBuffer logBuffer
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var failedFields = validationException.Errors.Select(e =>
            $"{e.PropertyName}:{e.ErrorCode}"
        );

        logger.LogWarning(
            "[{Handler}] Validation failed for fields: {Fields}",
            nameof(ValidationExceptionHandler),
            string.Join(", ", failedFields)
        );

        logBuffer.Flush();

        var errors = validationException
            .Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        await TypedResults
            .ValidationProblem(errors, title: "One or more validation errors occurred.")
            .ExecuteAsync(httpContext);

        return true;
    }
}

public static class ValidationExceptionHandlerExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the validation exception handler so FluentValidation failures are returned as standardized validation
        ///     problem responses.
        /// </summary>
        public void AddValidationExceptionHandler()
        {
            services.AddExceptionHandler<ValidationExceptionHandler>();
        }
    }
}
