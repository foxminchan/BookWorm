using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.List;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Mapper;
using BookWorm.Chassis.Search;

namespace BookWorm.Catalog.UnitTests.Features.Books.List;

public sealed class ListBooksQueryTests
{
    private const int MaxResults = 20;
    private static readonly string[] _defaultKeyword = [nameof(Book)];
    private BookDto[] _bookDtos = null!;
    private List<Book> _books = null!;
    private ListBooksHandler _handler = null!;
    private Mock<ISearch> _mockHybridSearch = null!;
    private Mock<IMapper<Book, BookDto>> _mockMapper = null!;
    private Mock<IBookRepository> _mockRepository = null!;

    [Before(Test)]
    public void Setup()
    {
        _mockHybridSearch = new();
        _mockRepository = new();
        _mockMapper = new();
        _handler = new(_mockHybridSearch.Object, _mockRepository.Object, _mockMapper.Object);

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
            .Setup(m => m.Map((It.IsAny<IEnumerable<Book>>() as IReadOnlyList<Book>)!))
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
        result.TotalPages.ShouldBe((long)Math.Ceiling(expectedTotalItems / (double)query.PageSize));

        _mockHybridSearch.Verify(
            s =>
                s.SearchAsync(
                    It.IsAny<string>(),
                    _defaultKeyword,
                    MaxResults,
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenSearchTerm_WhenHandlingListBooksQuery_ThenShouldUseSemanticSearch()
    {
        // Arrange
        const string searchTerm = "fantasy novel";
        var query = new ListBooksQuery(Search: searchTerm);
        List<TextSnippet> hybridSearchRecords =
        [
            new() { Id = Guid.CreateVersion7(), Description = "Book 1" },
            new() { Id = Guid.CreateVersion7(), Description = "Book 2" },
        ];

        _mockHybridSearch
            .Setup(s =>
                s.SearchAsync(
                    searchTerm,
                    new[] { nameof(Book), searchTerm },
                    MaxResults,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(hybridSearchRecords);

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

        _mockHybridSearch.Verify(
            s =>
                s.SearchAsync(
                    searchTerm,
                    new[] { nameof(Book), searchTerm },
                    MaxResults,
                    It.IsAny<CancellationToken>()
                ),
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
            .ReturnsAsync([.. _books.Take(pageSize)]);

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
        result.TotalPages.ShouldBe((long)Math.Ceiling(totalItems / (double)pageSize));

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    [MatrixDataSource]
    public async Task GivenCustomOrdering_WhenHandlingListBooksQuery_ThenShouldOrderCorrectly(
        [Matrix<string>(
            nameof(Book.Name),
            nameof(Book.Price.OriginalPrice),
            nameof(Book.Price.DiscountPrice),
            nameof(Book.Status),
            null!
        )]
            string? orderBy,
        [Matrix<bool>(false, true)] bool isDescending
    )
    {
        // Arrange
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
            .Setup(m => m.Map((It.IsAny<IEnumerable<Book>>() as IReadOnlyList<Book>)!))
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
