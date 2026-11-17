using System.Diagnostics.CodeAnalysis;
using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.Services;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpQueryAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => new HttpMethod("QUERY");
}

public sealed record ListBooksRequest(string? Search = null);

public interface ICatalogApi
{
    [HttpQuery("/api/v1/books")]
    Task<ApiResponse<PagedResult<Book>>> ListBooksAsync([Body] ListBooksRequest request);

    [Get("/api/v1/books/{id}")]
    Task<ApiResponse<Book>> GetBookAsync(BookId id);

    [Get("/api/v1/categories")]
    Task<ApiResponse<List<Category>>> ListCategoriesAsync();

    [Get("/api/v1/authors")]
    Task<ApiResponse<List<Author>>> ListAuthorsAsync();
}
