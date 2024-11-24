using BookWorm.Ordering.Constants;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Filters;

internal sealed class IdempotencyFilter(IRedisService redisService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var request = context.HttpContext.Request;
        var requestMethod = request.Method;
        var requestPath = request.Path;
        var requestId = request.Headers[Http.Idempotency].FirstOrDefault();

        if (requestMethod is not Http.Methods.Post and not Http.Methods.Patch)
        {
            return await next(context);
        }

        List<ValidationFailure> errors = [];

        if (string.IsNullOrEmpty(requestId))
        {
            errors.Add(
                new(
                    Http.Idempotency,
                    $"{Http.Idempotency} header is required for {Http.Methods.Post} and {Http.Methods.Patch} requests."
                )
            );
            throw new ValidationException(errors.AsEnumerable());
        }

        var cacheKey = $"{requestMethod}:{requestPath}:{requestId}";

        if (await redisService.GetAsync<Idempotent>(cacheKey) is not null)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            return TypedResults.Conflict("You have already requested this operation.");
        }

        Idempotent idempotent = new() { Id = cacheKey, Name = request.GetType().Name };

        await redisService.GetOrSetAsync(cacheKey, () => idempotent, TimeSpan.FromMinutes(1));

        return await next(context);
    }

    internal sealed class Idempotent
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

internal sealed class FromIdempotencyHeader : FromHeaderAttribute
{
    public new string Name => Http.Idempotency;
}
