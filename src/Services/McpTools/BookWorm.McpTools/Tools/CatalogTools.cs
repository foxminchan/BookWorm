using Grpc.Core;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class CatalogTools(IBookService bookService)
{
    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "search_catalog", Title = "Search BookWorm Catalog")]
    [Description("Searches the BookWorm catalog for a provided book description")]
    [return: Description("A JSON array of books matching the description or a not found message")]
    public async Task<string> SearchCatalogAsync(
        [Description("The product description for which to search")] string description,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Please provide a non-empty description to search the catalog.";
        }

        try
        {
            var books = await bookService.ListBooksAsync(description, cancellationToken);

            return books.Count == 0
                ? "We couldn't find any books matching your description. Please try again with a different description."
                : JsonSerializer.Serialize(
                    books,
                    BookSerializationContext.Default.IReadOnlyListBook
                );
        }
        catch (RpcException)
        {
            return "There was an error while searching the catalog. Please try again later.";
        }
    }

    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "get_book", Title = "Get Book Details")]
    [Description("Retrieves full details for a specific book by its unique identifier")]
    [return: Description("A JSON object with the book details or a not found message")]
    public async Task<string> GetBookAsync(
        [Description("The unique identifier of the book to retrieve")] Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            return "Please provide a valid non-empty book ID.";
        }

        try
        {
            var book = await bookService.GetBookByIdAsync(id, cancellationToken);

            return book is null
                ? $"No book found with ID {id}."
                : JsonSerializer.Serialize(book, BookSerializationContext.Default.Book);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return $"No book found with ID {id}.";
        }
        catch (RpcException)
        {
            return "There was an error retrieving the book. Please try again later.";
        }
    }

    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "list_categories", Title = "List Book Categories")]
    [Description("Retrieves all available book categories from the BookWorm catalog")]
    [return: Description("A JSON array of categories or a message when none are available")]
    public async Task<string> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await bookService.ListCategoriesAsync(cancellationToken);

            return categories.Count == 0
                ? "No categories are currently available."
                : JsonSerializer.Serialize(
                    categories,
                    BookSerializationContext.Default.IReadOnlyListCategory
                );
        }
        catch (RpcException)
        {
            return "There was an error retrieving categories. Please try again later.";
        }
    }

    [McpMeta("category", "catalog")]
    [McpServerTool(Name = "list_authors", Title = "List Book Authors")]
    [Description("Retrieves all authors available in the BookWorm catalog")]
    [return: Description("A JSON array of authors or a message when none are available")]
    public async Task<string> ListAuthorsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var authors = await bookService.ListAuthorsAsync(cancellationToken);

            return authors.Count == 0
                ? "No authors are currently available."
                : JsonSerializer.Serialize(
                    authors,
                    BookSerializationContext.Default.IReadOnlyListAuthor
                );
        }
        catch (RpcException)
        {
            return "There was an error retrieving authors. Please try again later.";
        }
    }
}

[JsonSerializable(typeof(IReadOnlyList<Book>))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(IReadOnlyList<Category>))]
[JsonSerializable(typeof(IReadOnlyList<Author>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class BookSerializationContext : JsonSerializerContext;
