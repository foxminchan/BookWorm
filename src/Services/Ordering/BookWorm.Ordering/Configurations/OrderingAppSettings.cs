using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.OpenApi;

namespace BookWorm.Ordering.Configurations;

[ExcludeFromCodeCoverage]
public sealed class OrderingAppSettings : AppSettings
{
    public override OpenApiInfo? OpenApi { get; set; } =
        new()
        {
            Title = "Ordering Service API",
            Summary =
                "A service responsible for managing the order lifecycle for the BookWorm platform",
            Description =
                "Orchestrates the entire order lifecycle, from creation to completion or cancellation, for the BookWorm platform",
            Contact = new()
            {
                Name = "Nhan Nguyen",
                Email = "nguyenxuannhan407@gmail.com",
                Url = new("https://github.com/foxminchan"),
            },
            License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") },
        };
}
