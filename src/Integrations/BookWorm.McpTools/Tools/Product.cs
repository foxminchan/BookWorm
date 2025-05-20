using System.Text.Json;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Product(ICatalogApi catalogApi)
{
    [McpServerTool(Name = "SearchCatalog")]
    [Description("Searches the BookWorm catalog for a provided book description")]
    public async Task<string> SearchCatalogAsync(
        [Description("The product description for which to search")] string description
    )
    {
        const string notFoundMessage =
            "We couldn't find any books matching your description. Please try again with a different description.";

        var response = await catalogApi.ListBooksAsync(description);

        return response.Items.Count == 0
            ? notFoundMessage
            : JsonSerializer.Serialize(response.Items);
    }
}
