using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using static BookWorm.Constants.Core.Http;

namespace BookWorm.Ordering.Infrastructure.Filters;

internal sealed class IdempotencyEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var request = context.HttpContext.Request;
        var requestMethod = request.Method;
        var requestPath = request.Path;
        var requestId = request.Headers[RequestIdHeader].FirstOrDefault();
        var requestManager =
            context.HttpContext.RequestServices.GetRequiredService<IRequestManager>();

        if (IsIdempotentMethod(requestMethod))
        {
            return await next(context);
        }

        if (string.IsNullOrWhiteSpace(requestId))
        {
            var error = new ValidationFailure(
                RequestIdHeader,
                $"{RequestIdHeader} header is required for {HttpMethods.Post} and {HttpMethods.Patch} requests."
            );
            throw new ValidationException([error]);
        }

        if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
        {
            return await next(context);
        }

        var idempotencyKey = $"{requestMethod}:{requestPath}:{requestId}";
        var cancellationToken = context.HttpContext.RequestAborted;

        var clientRequest = new ClientRequest
        {
            Id = idempotencyKey,
            Name = $"{requestMethod}:{requestPath}",
            Time = DateTime.UtcNow,
        };

        if (!await requestManager.TryCreateAsync(clientRequest, cancellationToken))
        {
            return TypedResults.Conflict(
                new ProblemDetails { Detail = "You have already requested this operation." }
            );
        }

        try
        {
            var response = await next(context);

            if (
                response
                    is IStatusCodeHttpResult
                    {
                        StatusCode: >= StatusCodes.Status500InternalServerError
                    }
                || context.HttpContext.Response.StatusCode
                    >= StatusCodes.Status500InternalServerError
            )
            {
                await requestManager.DeleteAsync(idempotencyKey, CancellationToken.None);
            }

            return response;
        }
        catch
        {
            await requestManager.DeleteAsync(idempotencyKey, CancellationToken.None);
            throw;
        }
    }

    private static bool IsIdempotentMethod(string method)
    {
        return HttpMethods.IsGet(method)
            || HttpMethods.IsDelete(method)
            || HttpMethods.IsPut(method)
            || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method);
    }
}

internal static class IdempotencyEndpointFilterExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        public TBuilder WithIdempotency()
        {
            builder.AddEndpointFilter(new IdempotencyEndpointFilter());
            return builder;
        }
    }
}
