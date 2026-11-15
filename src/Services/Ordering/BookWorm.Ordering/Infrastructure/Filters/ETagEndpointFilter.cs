using Microsoft.Net.Http.Headers;

namespace BookWorm.Ordering.Infrastructure.Filters;

public interface IHasVersion
{
    long Version { get; }
}

internal sealed class ETagEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var result = await next(context);

        var versioned = result switch
        {
            IHasVersion direct => direct,
            IValueHttpResult { Value: IHasVersion wrapped } => wrapped,
            _ => null,
        };

        if (versioned is null)
        {
            return result;
        }

        var etag = $"\"{versioned.Version}\"";
        context.HttpContext.Response.Headers.ETag = etag;

        if (
            context.HttpContext.Request.Headers.TryGetValue(
                HeaderNames.IfNoneMatch,
                out var ifNoneMatch
            )
            && EntityTagHeaderValue.TryParseList(ifNoneMatch, out var parsedTags)
            && parsedTags.Any(tag =>
                tag.Equals(EntityTagHeaderValue.Any)
                || string.Equals(tag.Tag.Value, etag, StringComparison.Ordinal)
            )
        )
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified);
        }

        return result;
    }
}

internal static class ETagEndpointExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder WithETag()
        {
            return builder
                .AddEndpointFilter<ETagEndpointFilter>()
                .Produces(StatusCodes.Status304NotModified);
        }
    }
}
