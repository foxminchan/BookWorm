using BookWorm.Ordering.Constants;
using FluentValidation.Results;

namespace BookWorm.Ordering.Filters;

public sealed class IdempotencyFilter(IRedisService redisService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        var requestMethod = request.Method;
        var requestPath = request.Path;
        var requestId = request.Headers[HeaderName.IdempotencyKey].FirstOrDefault();

        if (requestMethod is not "POST" and not "PATCH")
        {
            return await next(context);
        }

        List<ValidationFailure> errors = [];

        if (string.IsNullOrEmpty(requestId))
        {
            errors.Add(new(HeaderName.IdempotencyKey,
                $"{HeaderName.IdempotencyKey} header is required for POST and PATCH requests."));
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
