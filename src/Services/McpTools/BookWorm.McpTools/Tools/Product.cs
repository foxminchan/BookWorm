using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Product(ICatalogApi catalogApi)
{
    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "search_catalog", Title = "Search BookWorm Catalog")]
    [Description("Searches the BookWorm catalog for a provided book description")]
    [return: Description("A JSON array of books matching the description or a not found message")]
    public async Task<string> SearchCatalogAsync(
        [Description("The product description for which to search")] string description
    )
    {
        var response = await catalogApi.ListBooksAsync(description);

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error while searching the catalog. Please try again later.";
        }

        return response.Content?.Count == 0
            ? "We couldn't find any books matching your description. Please try again with a different description."
            : JsonSerializer.Serialize(
                response.Content,
                BookSerializationContext.Default.IReadOnlyListBook
            );
    }

    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "get_book", Title = "Get Book Details")]
    [Description("Retrieves full details for a specific book by its unique identifier")]
    [return: Description("A JSON object with the book details or a not found message")]
    public async Task<string> GetBookAsync(
        [Description("The unique identifier of the book to retrieve")] Guid id
    )
    {
        var response = await catalogApi.GetBookAsync(id);

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error retrieving the book. Please try again later.";
        }

        return response.Content is null
            ? $"No book found with ID {id}."
            : JsonSerializer.Serialize(response.Content, BookSerializationContext.Default.Book);
    }

    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "list_categories", Title = "List Book Categories")]
    [Description("Retrieves all available book categories from the BookWorm catalog")]
    [return: Description("A JSON array of categories or a message when none are available")]
    public async Task<string> ListCategoriesAsync()
    {
        var response = await catalogApi.ListCategoriesAsync();

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error retrieving categories. Please try again later.";
        }

        return response.Content is null or { Count: 0 }
            ? "No categories are currently available."
            : JsonSerializer.Serialize(
                response.Content,
                BookSerializationContext.Default.ListCategory
            );
    }

    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "list_authors", Title = "List Book Authors")]
    [Description("Retrieves all authors available in the BookWorm catalog")]
    [return: Description("A JSON array of authors or a message when none are available")]
    public async Task<string> ListAuthorsAsync()
    {
        var response = await catalogApi.ListAuthorsAsync();

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error retrieving authors. Please try again later.";
        }

        return response.Content is null or { Count: 0 }
            ? "No authors are currently available."
            : JsonSerializer.Serialize(
                response.Content,
                BookSerializationContext.Default.ListAuthor
            );
    }
}

[JsonSerializable(typeof(IReadOnlyList<Book>))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(List<Category>))]
[JsonSerializable(typeof(List<Author>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class BookSerializationContext : JsonSerializerContext;
