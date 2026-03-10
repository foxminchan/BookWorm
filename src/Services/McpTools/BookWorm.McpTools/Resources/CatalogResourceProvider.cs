using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Resources;

[McpServerResourceType]
public sealed class CatalogResourceProvider(ICatalogApi catalogApi)
{
    [McpServerResource(
        UriTemplate = "bookworm://catalog/categories",
        Name = "Book Categories",
        MimeType = MediaTypeNames.Application.Json
    )]
    [Description("All available book categories in the BookWorm catalog")]
    public async Task<string> GetCategoriesAsync()
    {
        var response = await catalogApi.ListCategoriesAsync();

        if (!response.IsSuccessStatusCode)
        {
            return "[]";
        }

        return response.Content is null or { Count: 0 }
            ? "[]"
            : JsonSerializer.Serialize(
                response.Content,
                CatalogResourceSerializationContext.Default.ListCategory
            );
    }

    [McpServerResource(
        UriTemplate = "bookworm://catalog/authors",
        Name = "Book Authors",
        MimeType = MediaTypeNames.Application.Json
    )]
    [Description("All authors available in the BookWorm catalog")]
    public async Task<string> GetAuthorsAsync()
    {
        var response = await catalogApi.ListAuthorsAsync();

        if (!response.IsSuccessStatusCode)
        {
            return "[]";
        }

        return response.Content is null or { Count: 0 }
            ? "[]"
            : JsonSerializer.Serialize(
                response.Content,
                CatalogResourceSerializationContext.Default.ListAuthor
            );
    }

    [McpServerResource(
        UriTemplate = "bookworm://catalog/books/{id}",
        Name = "Book Details",
        MimeType = MediaTypeNames.Application.Json
    )]
    [Description("Full details for a specific book by its unique identifier")]
    public async Task<string> GetBookAsync(
        [Description("The unique identifier of the book")] Guid id
    )
    {
        var response = await catalogApi.GetBookAsync(id);

        if (!response.IsSuccessStatusCode)
        {
            throw new McpException($"Book with ID {id} could not be retrieved.");
        }

        if (response.Content is null)
        {
            throw new McpException($"Book with ID {id} was not found.");
        }

        return JsonSerializer.Serialize(
            response.Content,
            CatalogResourceSerializationContext.Default.Book
        );
    }
}

[JsonSerializable(typeof(List<Category>))]
[JsonSerializable(typeof(List<Author>))]
[JsonSerializable(typeof(Book))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class CatalogResourceSerializationContext : JsonSerializerContext;
