using BookWorm.SharedKernel.Helpers;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using static BookWorm.Constants.Core.Restful;
using static BookWorm.Constants.Core.Restful.Methods;

namespace BookWorm.Ordering.Infrastructure.Filters;

public sealed class IdempotencyEndpointFilter : IEndpointFilter
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
                $"{RequestIdHeader} header is required for {Post} and {Patch} requests."
            );
            throw new ValidationException([error]);
        }

        var idempotencyKey = $"{requestMethod}:{requestPath}:{requestId}";
        var cancellationToken = context.HttpContext.RequestAborted;

        if (await requestManager.IsExistAsync(idempotencyKey, cancellationToken))
        {
            return TypedResults.Conflict(
                new ProblemDetails { Detail = "You have already requested this operation." }
            );
        }

        var clientRequest = new ClientRequest
        {
            Id = idempotencyKey,
            Name = $"{requestMethod}:{requestPath}",
            Time = DateTimeHelper.UtcNow(),
        };

        await requestManager.CreateAsync(clientRequest, cancellationToken);

        return await next(context);
    }

    private static bool IsIdempotentMethod(string method)
    {
        return method.ToUpperInvariant() is "GET" or "DELETE" or "PUT" or "HEAD" or "OPTIONS";
    }
}

public static class IdempotencyEndpointFilterExtensions
{
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new IdempotencyEndpointFilter());
        return builder;
    }
}
