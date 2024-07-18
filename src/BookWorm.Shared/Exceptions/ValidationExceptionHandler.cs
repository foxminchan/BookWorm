using Ardalis.Result;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookWorm.Shared.Exceptions;

public sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        logger.LogError(validationException, "[{Handler}] Exception occurred: {Message}",
            nameof(ValidationExceptionHandler), validationException.Message);

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = validationException.GetType().Name,
            Title = "Validation failed",
            Detail = "One or more validation errors has occurred"
        };

        if (validationException.Errors is not null)
        {
            problemDetails.Extensions["errors"] = Result.Invalid(validationException
                .Errors
                .Select(e => new ValidationError(
                    e.PropertyName,
                    e.ErrorMessage,
                    StatusCodes.Status400BadRequest.ToString(),
                    ValidationSeverity.Info
                )).ToList());
        }

        await TypedResults.BadRequest(problemDetails).ExecuteAsync(httpContext);

        return true;
    }
}
