﻿using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Books.List;
using BookWorm.Catalog.UnitTests.Builder;

namespace BookWorm.Catalog.UnitTests.Application.Books;

public sealed class ListBooksHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly ListBooksHandler _handler;
    private readonly Mock<IReadRepository<Book>> _repositoryMock;
    private static readonly float[] v = [0.1f, 0.2f, 0.3f, 0.4f, 0.5f];

    public ListBooksHandlerTests()
    {
        _repositoryMock = new();
        _aiServiceMock = new();
        _handler = new(_aiServiceMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task GivenSearchString_ShouldReturnBooksAndPagedInfo_WhenBooksExist()
    {
        // Arrange
        var query = new ListBooksQuery()
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Name",
            IsDescending = false,
            Search = "test search",
        };

        var vector = new Vector(v);
        _aiServiceMock
            .Setup(s => s.GetEmbeddingAsync(query.Search!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vector);

        var books = BookBuilder.WithDefaultValues();

        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(books);
        result.PagedInfo.TotalRecords.Should().Be(1);
        result.PagedInfo.TotalPages.Should().Be(1);
        _aiServiceMock.Verify(
            s => s.GetEmbeddingAsync(query.Search!, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenNoSearchString_ShouldReturnBooksAndPagedInfo_WhenBooksExist()
    {
        // Arrange
        var query = new ListBooksQuery()
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Name",
            IsDescending = false,
        };

        var books = BookBuilder.WithDefaultValues();

        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(books);
        result.PagedInfo.TotalRecords.Should().Be(1);
        result.PagedInfo.TotalPages.Should().Be(1);
        _aiServiceMock.Verify(
            s => s.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenNoBooks_ShouldReturnEmptyListAndPagedInfo()
    {
        // Arrange
        var query = new ListBooksQuery()
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Name",
            IsDescending = false,
        };

        _repositoryMock
            .Setup(r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();
        result.PagedInfo.TotalRecords.Should().Be(0);
        result.PagedInfo.TotalPages.Should().Be(0);
        _aiServiceMock.Verify(
            s => s.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [CombinatorialData]
    public void GivenInvalidQuery_WhenHandlingQuery_ThenShouldThrowException(
        [CombinatorialValues(-1, 0)] int pageIndex,
        [CombinatorialValues(-1, 0)] int pageSize,
        [CombinatorialValues((Status)99, (Status)100)] Status status
    )
    {
        // Arrange
        var query = new ListBooksQuery()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            OrderBy = "Name",
            IsDescending = false,
            Statuses = new[] { status },
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
        _aiServiceMock.Verify(
            s => s.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repositoryMock.Verify(
            r => r.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
