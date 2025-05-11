using System.Text.Json;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Product(ISearch search, IBookService bookService)
{
    private const string CollectionName = "book";

    [McpServerTool]
    [Description("Searches the BookWorm catalog for a provided book description")]
    public async Task<string> SearchCatalogAsync(
        [Description("The product description for which to search")] string description
    )
    {
        const string notFoundMessage =
            "We couldn't find any books matching your description. Please try again with a different description.";
        string[] keywords = ["book", "author", "publisher"];

        var response = await search.SearchAsync(description, keywords, CollectionName);

        if (response.Count == 0)
        {
            return notFoundMessage;
        }

        var ids = response.Select(r => r.Id).Select(id => id.ToString());

        var results = await bookService.GetBooksByIdsAsync(ids);

        return results is null ? notFoundMessage : JsonSerializer.Serialize(results);
    }
}
