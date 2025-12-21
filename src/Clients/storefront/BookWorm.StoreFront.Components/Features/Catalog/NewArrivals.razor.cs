using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Features.Catalog;

public sealed partial class NewArrivals : ComponentBase
{
    private List<Book> _books = [];
    private bool _isLoading = true;

    private static List<Book> GetBooks()
    {
        return
        [
            new(
                Guid.NewGuid(),
                "The Great Novel",
                "Epic story with unforgettable characters",
                "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=300&h=400&fit=crop",
                19.99m,
                29.99m,
                "Available",
                new(Guid.NewGuid(), "Fiction"),
                new(Guid.NewGuid(), "Classic Publishers"),
                [new(Guid.NewGuid(), "John Doe")],
                4.8,
                120
            ),
            new(
                Guid.NewGuid(),
                "Modern Classic",
                "Must-read contemporary literature",
                "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=300&h=400&fit=crop",
                24.99m,
                null,
                "Available",
                new(Guid.NewGuid(), "Literary Fiction"),
                new(Guid.NewGuid(), "Modern Press"),
                [new(Guid.NewGuid(), "Jane Smith")],
                4.9,
                87
            ),
            new(
                Guid.NewGuid(),
                "Adventure Tales",
                "Journey through exciting narratives",
                "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=300&h=400&fit=crop",
                17.99m,
                22.99m,
                "Available",
                new(Guid.NewGuid(), "Adventure"),
                new(Guid.NewGuid(), "Adventure Books"),
                [new(Guid.NewGuid(), "Mike Johnson")],
                4.7,
                203
            ),
            new(
                Guid.NewGuid(),
                "Bestselling Series",
                "Complete collection of award winners",
                "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=300&h=400&fit=crop",
                34.99m,
                null,
                "Available",
                new(Guid.NewGuid(), "Series"),
                new(Guid.NewGuid(), "Premium Publishing"),
                [new(Guid.NewGuid(), "Sarah Williams")],
                5.0,
                456
            ),
        ];
    }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await Task.Delay(500); // Simulate API call
        _books = [.. GetBooks()];
        _isLoading = false;
    }
}
