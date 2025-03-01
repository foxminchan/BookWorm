using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

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
        var requestId = request.Headers[Restful.RequestIdHeader].FirstOrDefault();
        var requestManager =
            context.HttpContext.RequestServices.GetRequiredService<IRequestManager>();

        if (requestMethod is not Restful.Methods.Post and not Restful.Methods.Patch)
        {
            return await next(context);
        }

        if (string.IsNullOrWhiteSpace(requestId))
        {
            var error = new ValidationFailure(
                Restful.RequestIdHeader,
                $"{Restful.RequestIdHeader} header is required for {Restful.Methods.Post} and {Restful.Methods.Patch} requests."
            );
            throw new ValidationException([error]);
        }

        var idempotencyKey = $"{requestMethod}:{requestPath}:{requestId}";

        if (await requestManager.IsExistAsync(idempotencyKey, CancellationToken.None))
        {
            return TypedResults.Conflict(
                new ProblemDetails { Detail = "You have already requested this operation." }
            );
        }

        var clientRequest = new ClientRequest
        {
            Id = idempotencyKey,
            Name = request.GetType().Name,
            Time = DateTime.UtcNow,
        };

        await requestManager.CreateAsync(clientRequest, CancellationToken.None);

        return await next(context);
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
