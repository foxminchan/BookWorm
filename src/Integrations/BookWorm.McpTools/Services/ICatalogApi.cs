using BookWorm.McpTools.Models;
using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.Services;

public interface ICatalogApi
{
    [Get("/api/v1/books")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<ApiResponse<PagedResult<Book>>> ListBooksAsync(string? search);
}
