using System.Globalization;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace BookWorm.Chassis.Endpoints;

public interface IHasVersion
{
    long Version { get; }
}

public interface IHasExpectedVersion
{
    uint? ExpectedVersion { get; set; }
}

public sealed record ETagMetadata(bool SupportsResponseETag, bool SupportsIfMatch);

internal sealed class ETagEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        if (context.HttpContext.Request.TryGetIfMatchVersion(out var expectedVersion))
        {
            foreach (var argument in context.Arguments)
            {
                if (argument is IHasExpectedVersion versionedCommand)
                {
                    versionedCommand.ExpectedVersion ??= expectedVersion;
                }
            }
        }

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

        var etag = $"\"{versioned.Version.ToString(CultureInfo.InvariantCulture)}\"";
        context.HttpContext.Response.Headers.ETag = etag;

        var ifNoneMatch = context.HttpContext.Request.GetTypedHeaders().IfNoneMatch;

        if (
            ifNoneMatch.Count > 0
            && ifNoneMatch.Any(tag =>
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

public static class ETagExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        /// <summary>
        ///     Applies <see cref="ETagEndpointFilter" /> to populate <see cref="IHasExpectedVersion" /> commands from
        ///     <c>If-Match</c> headers and emit <c>ETag</c> / <c>304 Not Modified</c> responses for <see cref="IHasVersion" />
        ///     results.
        /// </summary>
        public RouteHandlerBuilder WithETag()
        {
            builder.AddEndpointFilter<ETagEndpointFilter>();
            builder.Finally(endpointBuilder =>
            {
                var supportsResponseETag = endpointBuilder
                    .Metadata.OfType<IProducesResponseTypeMetadata>()
                    .Any(m => m.Type is not null && typeof(IHasVersion).IsAssignableFrom(m.Type));

                var supportsIfMatch =
                    endpointBuilder
                        .Metadata.OfType<IAcceptsMetadata>()
                        .Any(m =>
                            m.RequestType is not null
                            && typeof(IHasExpectedVersion).IsAssignableFrom(m.RequestType)
                        )
                    || endpointBuilder
                        .Metadata.OfType<MethodInfo>()
                        .SelectMany(m => m.GetParameters())
                        .Any(p => typeof(IHasExpectedVersion).IsAssignableFrom(p.ParameterType));

                endpointBuilder.Metadata.Add(
                    new ETagMetadata(supportsResponseETag, supportsIfMatch)
                );

                if (
                    supportsResponseETag
                    && !endpointBuilder
                        .Metadata.OfType<IProducesResponseTypeMetadata>()
                        .Any(m => m.StatusCode == StatusCodes.Status304NotModified)
                )
                {
                    endpointBuilder.Metadata.Add(
                        new ProducesResponseTypeMetadata(
                            StatusCodes.Status304NotModified,
                            typeof(void)
                        )
                    );
                }

                if (
                    supportsIfMatch
                    && !endpointBuilder
                        .Metadata.OfType<IProducesResponseTypeMetadata>()
                        .Any(m => m.StatusCode == StatusCodes.Status412PreconditionFailed)
                )
                {
                    endpointBuilder.Metadata.Add(
                        new ProducesResponseTypeMetadata(
                            StatusCodes.Status412PreconditionFailed,
                            typeof(ProblemDetails),
                            [MediaTypeNames.Application.ProblemJson]
                        )
                    );
                }
            });

            return builder;
        }
    }

    extension(HttpRequest request)
    {
        /// <summary>
        ///     Attempts to parse an <c>If-Match</c> header (strong <c>"123"</c> or weak <c>W/"123"</c>)
        ///     into a <see cref="uint" /> row version matching <c>AuditableEntity.RowVersion</c>.
        /// </summary>
        /// <param name="version">The parsed row version if successful; otherwise, 0.</param>
        /// <returns>
        ///     <see langword="true" /> if the <c>If-Match</c> header was successfully parsed; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool TryGetIfMatchVersion(out uint version)
        {
            version = 0;

            return request.Headers.TryGetValue(HeaderNames.IfMatch, out var rawValues)
                && TryParseETagVersion(rawValues.ToString(), out version);
        }
    }

    /// <summary>
    ///     Attempts to parse an ETag header value (strong <c>"123"</c> or weak <c>W/"123"</c>)
    ///     into a <see cref="uint" /> row version matching <c>AuditableEntity.RowVersion</c>.
    /// </summary>
    /// <param name="rawHeader">The raw ETag header value to parse.</param>
    /// <param name="version">The parsed row version if successful; otherwise, 0.</param>
    /// <returns><see langword="true" /> if the ETag was successfully parsed; otherwise, <see langword="false" />.</returns>
    public static bool TryParseETagVersion(string? rawHeader, out uint version)
    {
        version = 0;

        if (
            string.IsNullOrWhiteSpace(rawHeader)
            || !EntityTagHeaderValue.TryParse(rawHeader, out var etag)
        )
        {
            return false;
        }

        var tag = etag.Tag.Value;
        if (string.IsNullOrEmpty(tag))
        {
            return false;
        }

        return uint.TryParse(
            tag.AsSpan().Trim('"'),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out version
        );
    }
}
