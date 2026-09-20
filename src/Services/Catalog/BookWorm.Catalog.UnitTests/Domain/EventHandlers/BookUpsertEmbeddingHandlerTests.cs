using System.Reflection;
using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Domain.EventHandlers;
using BookWorm.Catalog.Domain.Events;
using BookWorm.Chassis.AI.Ingestion;
using BookWorm.SharedKernel.SeedWork;
using Microsoft.Extensions.Logging;

namespace BookWorm.Catalog.UnitTests.Domain.EventHandlers;

public sealed class BookUpsertEmbeddingHandlerTests
{
    private readonly BookUpsertEmbeddingHandler _handler;
    private readonly Mock<IIngestionSource<Book>> _ingestionMock = new();
    private readonly Mock<ILogger<BookUpsertEmbeddingHandler>> _loggerMock = new();

    public BookUpsertEmbeddingHandlerTests()
    {
        _handler = new(_loggerMock.Object, _ingestionMock.Object);
    }

    [Test]
    public async Task GivenBookCreatedEvent_WhenHandling_ThenShouldIngestData()
    {
        // Arrange
        var book = new Book(
            "Clean Code",
            "A handbook of agile software craftsmanship",
            "image.jpg",
            44.99m,
            39.99m,
            CategoryId.From(Guid.CreateVersion7()),
            PublisherId.From(Guid.CreateVersion7()),
            [AuthorId.From(Guid.CreateVersion7())]
        );
        typeof(Entity<BookId>)
            .GetProperty(
                nameof(Entity<>.Id),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
            )!
            .SetValue(book, BookId.From(Guid.CreateVersion7()));
        var @event = new BookCreatedEvent(book);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _ingestionMock.Verify(
            x => x.IngestDataAsync(book, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenBookUpdatedEvent_WhenHandling_ThenShouldIngestData()
    {
        // Arrange
        var book = new Book(
            "Refactoring",
            "Improving the Design of Existing Code",
            "refactoring.jpg",
            49.99m,
            44.99m,
            CategoryId.From(Guid.CreateVersion7()),
            PublisherId.From(Guid.CreateVersion7()),
            [AuthorId.From(Guid.CreateVersion7())]
        );
        typeof(Entity<BookId>)
            .GetProperty(
                nameof(Entity<>.Id),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
            )!
            .SetValue(book, BookId.From(Guid.CreateVersion7()));
        var @event = new BookUpdatedEvent(book);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _ingestionMock.Verify(
            x => x.IngestDataAsync(book, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
