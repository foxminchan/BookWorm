using BookWorm.McpTools.Models;
using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.Services;

public interface ICatalogApi
{
    [Get("/api/v1/books")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<ApiResponse<PagedResult<Book>>> ListBooksAsync(string? search);

    [Get("/api/v1/books/{id}")]
    Task<ApiResponse<Book>> GetBookAsync(Guid id);

    [Get("/api/v1/categories")]
    Task<ApiResponse<List<Category>>> ListCategoriesAsync();

    [Get("/api/v1/authors")]
    Task<ApiResponse<List<Author>>> ListAuthorsAsync();
}
