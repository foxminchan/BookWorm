using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chassis.Mapper;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace BookWorm.Catalog.UnitTests.Features.Books.Get;

public sealed class GetBookQueryTests : IDisposable
{
    private readonly FusionCache _cache;
    private readonly GetBookHandler _handler;
    private readonly Mock<IMapper<CachedBook, BookDto>> _mapperMock;
    private readonly Mock<IBookRepository> _repositoryMock;

    public GetBookQueryTests()
    {
        _repositoryMock = new();
        _mapperMock = new();
        _cache = new(new FusionCacheOptions());

        _handler = new(_repositoryMock.Object, _cache, _mapperMock.Object);
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task GivenValidId_WhenGetBookQueryHandled_ThenShouldReturnBookDto()
    {
        // Arrange
        var bookFaker = new BookFaker();
        var book = bookFaker.Generate(1)[0];
        var expectedBookDto = new BookDto(
            book.Id,
            book.Name,
            book.Description,
            book.Image,
            book.Price?.OriginalPrice ?? 0,
            book.Price?.DiscountPrice,
            book.Status,
            null, // Category
            null, // Publisher
            [], // Authors
            0, // AverageRating
            0 // TotalReviews
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync((Guid)book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(m => m.Map(It.Is<CachedBook>(cb => cb.Id == book.Id)))
            .Returns(expectedBookDto);

        // Act
        var result = await _handler.Handle(new(book.Id), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedBookDto);
        _repositoryMock.Verify(
            r => r.GetByIdAsync((Guid)book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mapperMock.Verify(m => m.Map(It.Is<CachedBook>(cb => cb.Id == book.Id)), Times.Once);
    }

    [Test]
    public async Task GivenCachedBook_WhenGetBookQueryHandledTwice_ThenShouldNotHitRepositoryAgain()
    {
        // Arrange
        var book = new BookFaker().Generate(1)[0];
        var expectedBookDto = new BookDto(
            book.Id,
            book.Name,
            book.Description,
            book.Image,
            book.Price?.OriginalPrice ?? 0,
            book.Price?.DiscountPrice,
            book.Status,
            null,
            null,
            [],
            0,
            0
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync((Guid)book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _mapperMock
            .Setup(m => m.Map(It.Is<CachedBook>(cb => cb.Id == book.Id)))
            .Returns(expectedBookDto);

        // Act
        var first = await _handler.Handle(new(book.Id), CancellationToken.None);
        var second = await _handler.Handle(new(book.Id), CancellationToken.None);

        // Assert
        first.ShouldBe(expectedBookDto);
        second.ShouldBe(expectedBookDto);
        _repositoryMock.Verify(
            r => r.GetByIdAsync((Guid)book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mapperMock.Verify(m => m.Map(It.Is<CachedBook>(cb => cb.Id == book.Id)), Times.Exactly(2));
    }

    [Test]
    public async Task GivenCachedBook_WhenSerializedWithFusionCacheSerializer_ThenShouldRoundTripAllFields()
    {
        // Arrange
        var category = new Category("Software") { Id = CategoryId.From(Guid.CreateVersion7()) };
        var publisher = new Publisher("Tech Press")
        {
            Id = PublisherId.From(Guid.CreateVersion7()),
        };
        var author = new Author("Ada Lovelace") { Id = AuthorId.From(Guid.CreateVersion7()) };
        var book = new Book(
            "Clean Architecture",
            "A craftsman's guide to software structure.",
            "urn:account:catalog:cover.png",
            49.99m,
            39.99m,
            category.Id,
            publisher.Id,
            [author.Id]
        )
        {
            Id = BookId.From(Guid.CreateVersion7()),
            RowVersion = 42,
        };
        typeof(Book).GetProperty(nameof(Book.Category))?.SetValue(book, category);
        typeof(Book).GetProperty(nameof(Book.Publisher))?.SetValue(book, publisher);
        typeof(BookAuthor)
            .GetProperty(nameof(BookAuthor.Author))
            ?.SetValue(book.BookAuthors.Single(), author);
        book.AddRating(5);

        var cachedBook = CachedBook.FromBook(book);
        var serializer = new FusionCacheSystemTextJsonSerializer();

        // Act
        var bytes = await serializer.SerializeAsync(cachedBook);
        var deserialized = await serializer.DeserializeAsync<CachedBook>(bytes);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Id.ShouldBe(cachedBook.Id);
        deserialized.Name.ShouldBe(cachedBook.Name);
        deserialized.Description.ShouldBe(cachedBook.Description);
        deserialized.Image.ShouldBe("urn:account:catalog:cover.png");
        deserialized.Price.ShouldBe(49.99m);
        deserialized.PriceSale.ShouldBe(39.99m);
        deserialized.Status.ShouldBe(Status.InStock);
        deserialized.Category.ShouldBe(new(category.Id, category.Name));
        deserialized.Publisher.ShouldBe(new(publisher.Id, publisher.Name));
        deserialized.Authors.ShouldBe([new(author.Id, author.Name)]);
        deserialized.AverageRating.ShouldBe(5.0);
        deserialized.TotalReviews.ShouldBe(1);
        deserialized.Version.ShouldBe(42);
    }

    [Test]
    public async Task GivenNonExistentId_WhenGetBookQueryHandled_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = BookId.From(Guid.CreateVersion7());
        _repositoryMock
            .Setup(r => r.GetByIdAsync((Guid)nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book)null!);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(new(nonExistentId), CancellationToken.None)
        );

        // Assert
        exception.Message.ShouldBe($"Book with id {nonExistentId} not found.");
        _repositoryMock.Verify(
            r => r.GetByIdAsync((Guid)nonExistentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mapperMock.Verify(m => m.Map(It.IsAny<CachedBook>()), Times.Never);
    }
}
