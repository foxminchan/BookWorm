using BookWorm.SharedKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace BookWorm.Chassis.Endpoints;

internal sealed class PaginationHeaderFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
        {
            return await next(context);
        }

        var result = await next(context);

        if (result is null)
        {
            return result;
        }

        var pagedResult = result switch
        {
            IPagedResult direct => direct,
            IValueHttpResult { Value: IPagedResult wrapped } => wrapped,
            _ => null,
        };

        if (pagedResult is null)
        {
            return result;
        }

        AddLinkHeader(context.HttpContext, pagedResult);

        return result;
    }

    private static void AddLinkHeader(HttpContext context, IPagedResult metadata)
    {
        var request = context.Request;
        var basePath = request.Path.Value ?? "/";
        var queryParams = QueryHelpers.ParseQuery(request.QueryString.Value ?? string.Empty);

        List<string> links =
        [
            BuildLink(basePath, queryParams, 1, metadata.PageSize, "first"),
            BuildLink(basePath, queryParams, metadata.PageIndex, metadata.PageSize, "self"),
        ];

        if (metadata is { HasPreviousPage: true, PageIndex: > 1 })
        {
            links.Add(
                BuildLink(
                    basePath,
                    queryParams,
                    metadata.PageIndex - 1,
                    metadata.PageSize,
                    "previous"
                )
            );
        }

        if (metadata.HasNextPage && metadata.PageIndex < metadata.TotalPages)
        {
            links.Add(
                BuildLink(basePath, queryParams, metadata.PageIndex + 1, metadata.PageSize, "next")
            );
        }

        links.Add(
            BuildLink(basePath, queryParams, (int)metadata.TotalPages, metadata.PageSize, "last")
        );

        context.Response.Headers.Link = new([.. links]);
        context.Response.Headers.Append("Pagination-Count", metadata.TotalItems.ToString());
    }

    private static string BuildLink(
        string basePath,
        IDictionary<string, StringValues> queryParams,
        int pageIndex,
        int pageSize,
        string rel
    )
    {
        Dictionary<string, string?> updatedParams =
        [
            with(
                queryParams
                    .Where(kvp =>
                        !kvp.Key.Equals("pageIndex", StringComparison.OrdinalIgnoreCase)
                        && !kvp.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase)
                    )
                    .Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value.ToString()))
            ),
        ];
        updatedParams["pageIndex"] = pageIndex.ToString();
        updatedParams["pageSize"] = pageSize.ToString();

        var queryString = QueryHelpers.AddQueryString(basePath, updatedParams);
        return $"<{queryString}>;rel={rel}";
    }
}

public static class PaginationHeaderFilterExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        ///     Adds the pagination header endpoint filter to the current endpoint builder.
        /// </summary>
        /// <returns>
        ///     The same endpoint builder instance to support fluent chaining.
        /// </returns>
        public TBuilder WithPaginationHeaders()
        {
            builder.AddEndpointFilter(new PaginationHeaderFilter());
            return builder;
        }
    }
}
