using BookWorm.SharedKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace BookWorm.Chassis.Endpoints;

public sealed class PaginationHeaderFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var result = await next(context);

        if (result is null)
        {
            return result;
        }

        object? pagedResult = null;

        var resultType = result.GetType();
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Ok<>))
        {
            var valueProperty = resultType.GetProperty("Value");
            var value = valueProperty?.GetValue(result);
            if (value is not null && IsPagedResultType(value.GetType()))
            {
                pagedResult = value;
            }
        }
        else if (IsPagedResultType(resultType))
        {
            pagedResult = result;
        }

        if (pagedResult is null)
        {
            return result;
        }

        var metadata = ExtractPaginationMetadata(pagedResult);
        AddLinkHeader(context.HttpContext, metadata);

        return result;
    }

    private static bool IsPagedResultType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedResult<>);
    }

    private static PaginationMetadata ExtractPaginationMetadata(object pagedResult)
    {
        var type = pagedResult.GetType();

        return new(
            PageIndex: Convert.ToInt32(
                type.GetProperty(nameof(PagedResult<>.PageIndex))?.GetValue(pagedResult)
            ),
            PageSize: Convert.ToInt32(
                type.GetProperty(nameof(PagedResult<>.PageSize))?.GetValue(pagedResult)
            ),
            TotalItems: Convert.ToInt32(
                type.GetProperty(nameof(PagedResult<>.TotalItems))?.GetValue(pagedResult)
            ),
            TotalPages: Convert.ToInt32(
                type.GetProperty(nameof(PagedResult<>.TotalPages))?.GetValue(pagedResult)
            ),
            HasPreviousPage: (bool?)
                type.GetProperty(nameof(PagedResult<>.HasPreviousPage))?.GetValue(pagedResult)
                ?? false,
            HasNextPage: (bool?)
                type.GetProperty(nameof(PagedResult<>.HasNextPage))?.GetValue(pagedResult)
                ?? false
        );
    }

    private static void AddLinkHeader(HttpContext context, PaginationMetadata metadata)
    {
        var request = context.Request;
        var basePath = request.Path.Value ?? "/";
        var queryParams = QueryHelpers.ParseQuery(request.QueryString.Value ?? string.Empty);

        var links = new List<string>
        {
            BuildLink(basePath, queryParams, 1, metadata.PageSize, "first"),
            BuildLink(basePath, queryParams, metadata.PageIndex, metadata.PageSize, "self"),
        };

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

        links.Add(BuildLink(basePath, queryParams, metadata.TotalPages, metadata.PageSize, "last"));

        context.Response.Headers.Append("Link", string.Join(",", links));
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
        var updatedParams = new Dictionary<string, string?>(
            queryParams
                .Where(kvp =>
                    !kvp.Key.Equals("pageIndex", StringComparison.OrdinalIgnoreCase)
                    && !kvp.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase)
                )
                .Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value.ToString()))
        )
        {
            ["pageIndex"] = pageIndex.ToString(),
            ["pageSize"] = pageSize.ToString(),
        };

        var queryString = QueryHelpers.AddQueryString(basePath, updatedParams);
        return $"<{queryString}>;rel={rel}";
    }

    private sealed record PaginationMetadata(
        int PageIndex,
        int PageSize,
        int TotalItems,
        int TotalPages,
        bool HasPreviousPage,
        bool HasNextPage
    );
}

public static class PaginationHeaderFilterExtensions
{
    public static TBuilder WithPaginationHeaders<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new PaginationHeaderFilter());
        return builder;
    }
}
