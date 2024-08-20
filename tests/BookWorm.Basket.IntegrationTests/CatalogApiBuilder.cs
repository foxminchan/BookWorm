using WireMock.Client.Builders;

namespace BookWorm.Basket.IntegrationTests;

internal sealed class CatalogApiBuilder
{
    public static async Task BuildAsync(AdminApiMappingBuilder builder)
    {
        await builder.BuildAndPostAsync();
    }
}
