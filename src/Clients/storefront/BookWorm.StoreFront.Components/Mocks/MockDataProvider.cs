using BookWorm.StoreFront.Components.Models;

namespace BookWorm.StoreFront.Components.Mocks;

public static class MockDataProvider
{
    public static List<Book> GetAllBooks()
    {
        return
        [
            new Book(
                Guid.NewGuid(),
                "Professional Drone",
                "4K camera with 30-minute flight time",
                "https://images.unsplash.com/photo-1508614999368-9260051292e5?w=400",
                399.99m,
                499.99m,
                "Available",
                new Category(Guid.NewGuid(), "Electronics"),
                new Publisher(Guid.NewGuid(), "Apple Inc."),
                [new Author(Guid.NewGuid(), "Tech Author")],
                4.5,
                218
            ),
            new Book(
                Guid.NewGuid(),
                "Digital Camera",
                "24MP with 4K video recording",
                "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=400",
                349.99m,
                null,
                "Available",
                new Category(Guid.NewGuid(), "Electronics"),
                new Publisher(Guid.NewGuid(), "Samsung"),
                [new Author(Guid.NewGuid(), "Camera Expert")],
                4.0,
                95
            ),
            new Book(
                Guid.NewGuid(),
                "Mechanical Keyboard",
                "Blue switches with RGB lighting",
                "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400",
                129.99m,
                null,
                "Available",
                new Category(Guid.NewGuid(), "Accessories"),
                new Publisher(Guid.NewGuid(), "Google"),
                [new Author(Guid.NewGuid(), "Keyboard Pro")],
                4.8,
                142
            ),
            new Book(
                Guid.NewGuid(),
                "Travel Backpack",
                "Water resistant with laptop compartment",
                "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400",
                59.99m,
                79.99m,
                "Available",
                new Category(Guid.NewGuid(), "Accessories"),
                new Publisher(Guid.NewGuid(), "Sony"),
                [new Author(Guid.NewGuid(), "Travel Writer")],
                4.6,
                324
            ),
            new Book(
                Guid.NewGuid(),
                "Indoor Plant",
                "Low maintenance air purifying plant",
                "https://images.unsplash.com/photo-1509937528035-ad76254b0356?w=400",
                34.99m,
                null,
                "Available",
                new Category(Guid.NewGuid(), "Home & Garden"),
                new Publisher(Guid.NewGuid(), "Apple Inc."),
                [new Author(Guid.NewGuid(), "Botanist")],
                4.2,
                87
            ),
            new Book(
                Guid.NewGuid(),
                "Smart Speaker",
                "Voice control with premium sound",
                "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400",
                99.99m,
                129.99m,
                "Available",
                new Category(Guid.NewGuid(), "Electronics"),
                new Publisher(Guid.NewGuid(), "Samsung"),
                [new Author(Guid.NewGuid(), "Audio Expert")],
                4.4,
                203
            ),
            new Book(
                Guid.NewGuid(),
                "Premium Headphones",
                "Noise cancelling with 30-hour battery",
                "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
                149.99m,
                199.99m,
                "Available",
                new Category(Guid.NewGuid(), "Electronics"),
                new Publisher(Guid.NewGuid(), "Sony"),
                [new Author(Guid.NewGuid(), "Sound Engineer")],
                4.7,
                512
            ),
            new Book(
                Guid.NewGuid(),
                "Athletic Sneakers",
                "Lightweight with cushioned support",
                "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400",
                89.99m,
                119.99m,
                "Available",
                new Category(Guid.NewGuid(), "Fashion"),
                new Publisher(Guid.NewGuid(), "Google"),
                [new Author(Guid.NewGuid(), "Sports Expert")],
                4.3,
                876
            ),
            new Book(
                Guid.NewGuid(),
                "Premium Coffee Maker",
                "12-cup capacity with programmable timer",
                "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=400",
                79.99m,
                null,
                "Available",
                new Category(Guid.NewGuid(), "Home & Kitchen"),
                new Publisher(Guid.NewGuid(), "Apple Inc."),
                [new Author(Guid.NewGuid(), "Coffee Expert")],
                4.5,
                342
            ),
        ];
    }

    public static List<Book> GetNewArrivals()
    {
        return [.. GetAllBooks().Take(4)];
    }

    public static List<Publisher> GetPublishers()
    {
        return
        [
            new Publisher(Guid.NewGuid(), "Apple Inc."),
            new Publisher(Guid.NewGuid(), "Samsung"),
            new Publisher(Guid.NewGuid(), "Google"),
            new Publisher(Guid.NewGuid(), "Sony"),
        ];
    }
}
