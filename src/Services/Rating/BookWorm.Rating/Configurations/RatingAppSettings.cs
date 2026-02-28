using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.OpenApi;

namespace BookWorm.Rating.Configurations;

[ExcludeFromCodeCoverage]
public sealed class RatingAppSettings : AppSettings
{
    public override OpenApiInfo? OpenApi { get; set; } =
        new()
        {
            Title = "Rating Service API",
            Summary =
                "A service responsible for managing user feedback and ratings for books on the BookWorm platform",
            Description =
                "Handles the collection, storage, and aggregation of user feedback and ratings for books on the BookWorm platform",
            Contact = new()
            {
                Name = "Nhan Nguyen",
                Email = "nguyenxuannhan407@gmail.com",
                Url = new("https://github.com/foxminchan"),
            },
            License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") },
        };
}
