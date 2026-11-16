using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.Services;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpQueryAttribute(string path) : HttpMethodAttribute(path)
{
#pragma warning disable IDE0090 // Refit source generator requires explicit new HttpMethod("QUERY") for generated-only clients
    public override HttpMethod Method => new HttpMethod("QUERY");
#pragma warning restore IDE0090
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
