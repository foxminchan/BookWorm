using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Home;

public sealed partial class NewArrivals : ComponentBase
{
    private readonly List<Book> _books = GetBooks();

    private static List<Book> GetBooks()
    {
        return
        [
            new Book(
                Id: Guid.NewGuid(),
                Name: "The Great Novel",
                Description: "Epic story with unforgettable characters",
                ImageUrl: "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=300&h=400&fit=crop",
                Price: 19.99m,
                PriceSale: 29.99m,
                Status: "Available",
                Category: new Category(Guid.NewGuid(), "Fiction"),
                Publisher: new Publisher(Guid.NewGuid(), "Classic Publishers"),
                Authors: [new Author(Guid.NewGuid(), "John Doe")],
                AverageRating: 4.8,
                TotalReviews: 120,
                CreatedAt: DateTime.Now.AddDays(-5)
            ),
            new Book(
                Id: Guid.NewGuid(),
                Name: "Modern Classic",
                Description: "Must-read contemporary literature",
                ImageUrl: "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=300&h=400&fit=crop",
                Price: 24.99m,
                PriceSale: null,
                Status: "Available",
                Category: new Category(Guid.NewGuid(), "Literary Fiction"),
                Publisher: new Publisher(Guid.NewGuid(), "Modern Press"),
                Authors: [new Author(Guid.NewGuid(), "Jane Smith")],
                AverageRating: 4.9,
                TotalReviews: 87,
                CreatedAt: DateTime.Now.AddDays(-3)
            ),
            new Book(
                Id: Guid.NewGuid(),
                Name: "Adventure Tales",
                Description: "Journey through exciting narratives",
                ImageUrl: "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=300&h=400&fit=crop",
                Price: 17.99m,
                PriceSale: 22.99m,
                Status: "Available",
                Category: new Category(Guid.NewGuid(), "Adventure"),
                Publisher: new Publisher(Guid.NewGuid(), "Adventure Books"),
                Authors: [new Author(Guid.NewGuid(), "Mike Johnson")],
                AverageRating: 4.7,
                TotalReviews: 203,
                CreatedAt: DateTime.Now.AddDays(-7)
            ),
            new Book(
                Id: Guid.NewGuid(),
                Name: "Bestselling Series",
                Description: "Complete collection of award winners",
                ImageUrl: "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=300&h=400&fit=crop",
                Price: 34.99m,
                PriceSale: null,
                Status: "Available",
                Category: new Category(Guid.NewGuid(), "Series"),
                Publisher: new Publisher(Guid.NewGuid(), "Premium Publishing"),
                Authors: [new Author(Guid.NewGuid(), "Sarah Williams")],
                AverageRating: 5.0,
                TotalReviews: 456,
                CreatedAt: DateTime.Now.AddDays(-2)
            ),
        ];
    }

    // private async Task<List<Book>> GetBooksAsync()
    // {
    // 	return await _catalogService.GetNewArrivalsAsync();
    // }
}
