using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.OpenApi;

namespace BookWorm.Catalog.Configurations;

[ExcludeFromCodeCoverage]
internal sealed class CatalogAppSettings : AppSettings
{
    public override OpenApiInfo? OpenApi { get; set; } =
        new()
        {
            Title = "Catalog Service API",
            Summary =
                "A service responsible for managing the catalog of products for the BookWorm platform",
            Description =
                "Manages the catalog of products for the BookWorm platform, including books, authors, and categories",
            Contact = new()
            {
                Name = "BookWorm Engineering",
                Email = "engineering@bookworm.com",
                Url = new("https://github.com/foxminchan"),
            },
            License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") },
        };

    public int SasExpiryHours { get; set; }
}
