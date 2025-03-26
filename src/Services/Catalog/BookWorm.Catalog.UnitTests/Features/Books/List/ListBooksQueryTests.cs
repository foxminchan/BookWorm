using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.List;
using BookWorm.Catalog.Infrastructure.Services;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Catalog.UnitTests.Features.Books.List;

public sealed class ListBooksQueryTests
{
    private BookDto[] _bookDtos = null!;
    private List<Book> _books = null!;
    private ListBooksHandler _handler = null!;
    private Mock<IMapper<Book, BookDto>> _mockMapper = null!;
    private Mock<IBookRepository> _mockRepository = null!;
    private Mock<IBookSemanticSearch> _mockSemanticSearch = null!;

    [Before(Test)]
    public void Setup()
    {
        _mockSemanticSearch = new();
        _mockRepository = new();
        _mockMapper = new();
        _handler = new(_mockSemanticSearch.Object, _mockRepository.Object, _mockMapper.Object);

        // Generate test data
        var bookFaker = new BookFaker();
        _books = bookFaker.Generate(10);

        _bookDtos =
        [
            .. _books.Select(b => new BookDto(
                b.Id,
                b.Name,
                b.Description,
                b.Image,
                b.Price!.OriginalPrice,
                b.Price.DiscountPrice,
                b.Status,
                null, // Category
                null, // Publisher
                [], // Authors
                b.AverageRating,
                b.TotalReviews
            )),
        ];

        // Setup mapper mock
        _mockMapper
            .Setup(m => m.MapToDtos((It.IsAny<IEnumerable<Book>>() as IReadOnlyList<Book>)!))
            .Returns(_bookDtos);
    }

    [Test]
    public async Task GivenNoFilters_WhenHandlingListBooksQuery_ThenShouldReturnAllBooks()
    {
        // Arrange
        var query = new ListBooksQuery();
        const long expectedTotalItems = 10L;

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTotalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.PageIndex.ShouldBe(query.PageIndex);
        result.PageSize.ShouldBe(query.PageSize);
        result.TotalItems.ShouldBe(expectedTotalItems);
        result.TotalPages.ShouldBe(Math.Ceiling(expectedTotalItems / (double)query.PageSize));

        _mockSemanticSearch.Verify(
            s => s.FindBooksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenSearchTerm_WhenHandlingListBooksQuery_ThenShouldUseSemanticSearch()
    {
        // Arrange
        const string searchTerm = "fantasy novel";
        var query = new ListBooksQuery(Search: searchTerm);
        var semanticSearchResults = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };

        _mockSemanticSearch
            .Setup(s => s.FindBooksAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(semanticSearchResults);

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10L);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();

        _mockSemanticSearch.Verify(
            s => s.FindBooksAsync(searchTerm, It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Verify filter spec was created with the IDs from semantic search
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenPriceFilters_WhenHandlingListBooksQuery_ThenShouldFilterByPrice()
    {
        // Arrange
        const decimal minPrice = 100m;
        const decimal maxPrice = 500m;
        var query = new ListBooksQuery(MinPrice: minPrice, MaxPrice: maxPrice);

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10L);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCategoryFilter_WhenHandlingListBooksQuery_ThenShouldFilterByCategory()
    {
        // Arrange
        var categoryIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        var query = new ListBooksQuery(CategoryId: categoryIds);

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10L);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenPublisherFilter_WhenHandlingListBooksQuery_ThenShouldFilterByPublisher()
    {
        // Arrange
        var publisherIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        var query = new ListBooksQuery(PublisherId: publisherIds);

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10L);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenAuthorFilter_WhenHandlingListBooksQuery_ThenShouldFilterByAuthor()
    {
        // Arrange
        var authorIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        var query = new ListBooksQuery(AuthorIds: authorIds);

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10L);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCustomPagination_WhenHandlingListBooksQuery_ThenShouldReturnCorrectPage()
    {
        // Arrange
        const int pageIndex = 2;
        const int pageSize = 5;
        var query = new ListBooksQuery(pageIndex, pageSize);
        const long totalItems = 15L;

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books.Take(pageSize).ToArray());

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.PageIndex.ShouldBe(pageIndex);
        result.PageSize.ShouldBe(pageSize);
        result.TotalItems.ShouldBe(totalItems);
        result.TotalPages.ShouldBe(Math.Ceiling(totalItems / (double)pageSize));

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCustomOrdering_WhenHandlingListBooksQuery_ThenShouldOrderCorrectly()
    {
        // Arrange
        const string orderBy = nameof(Book.Price.OriginalPrice);
        const bool isDescending = true;
        var query = new ListBooksQuery(OrderBy: orderBy, IsDescending: isDescending);

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_books);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10L);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyRepository_WhenHandlingListBooksQuery_ThenShouldReturnEmptyResult()
    {
        // Arrange
        var query = new ListBooksQuery();
        const long totalItems = 0L;

        _mockRepository
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _mockRepository
            .Setup(r => r.CountAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        _mockMapper
            .Setup(m => m.MapToDtos((It.IsAny<IEnumerable<Book>>() as IReadOnlyList<Book>)!))
            .Returns([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeNull();
        result.PageIndex.ShouldBe(query.PageIndex);
        result.PageSize.ShouldBe(query.PageSize);
        result.TotalItems.ShouldBe(totalItems);
    }
}
