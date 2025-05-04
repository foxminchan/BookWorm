using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Exceptions;

public sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    : IExceptionHandler
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

        logger.LogError(
            validationException,
            "[{Handler}] Exception occurred: {Message}",
            nameof(ValidationExceptionHandler),
            validationException.Message
        );

        ProblemDetails problemDetails = new()
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors has occurred",
        };

        if (validationException.Errors is not null)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        await TypedResults.BadRequest(problemDetails).ExecuteAsync(httpContext);

        return true;
    }
}
