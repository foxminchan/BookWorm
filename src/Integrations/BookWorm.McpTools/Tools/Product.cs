using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Product(ICatalogApi catalogApi)
{
    [McpServerTool(Name = "SearchCatalog", Title = "Search BookWorm Catalog")]
    [Description("Searches the BookWorm catalog for a provided book description")]
    public async Task<string> SearchCatalogAsync(
        [Description("The product description for which to search")] string description
    )
    {
        const string notFoundMessage =
            "We couldn't find any books matching your description. Please try again with a different description.";

        var response = await catalogApi.ListBooksAsync(description);

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error while searching the catalog. Please try again later.";
        }

        return response.Content?.Items.Count == 0
            ? notFoundMessage
            : JsonSerializer.Serialize(
                response.Content?.Items,
                BookSerializationContext.Default.IReadOnlyListBook
            );
    }
}

[JsonSerializable(typeof(IReadOnlyList<Book>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class BookSerializationContext : JsonSerializerContext;
