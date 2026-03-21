using BookWorm.SharedKernel.Helpers;
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
        return HttpMethods.IsGet(method)
            || HttpMethods.IsDelete(method)
            || HttpMethods.IsPut(method)
            || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method);
    }
}

internal static class IdempotencyEndpointFilterExtensions
{
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new IdempotencyEndpointFilter());
        return builder;
    }
}
