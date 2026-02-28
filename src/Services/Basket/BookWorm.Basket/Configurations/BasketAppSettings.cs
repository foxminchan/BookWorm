using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.OpenApi;

namespace BookWorm.Basket.Configurations;

[ExcludeFromCodeCoverage]
public sealed class BasketAppSettings : AppSettings
{
    public override OpenApiInfo? OpenApi { get; set; } =
        new()
        {
            Title = "Basket Service API",
            Summary = "A service responsible for managing customer shopping baskets",
            Description =
                "Manages the lifecycle of customer shopping baskets, including adding and removing items, calculating totals, and handling checkout processes",
            Contact = new()
            {
                Name = "Nhan Nguyen",
                Email = "nguyenxuannhan407@gmail.com",
                Url = new("https://github.com/foxminchan"),
            },
            License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") },
        };
}
