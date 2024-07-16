using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Features.Authors.List;
using BookWorm.Catalog.UnitTests.Builder;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.UnitTests.Application.Authors;

public sealed class ListAuthorHandlerTests
{
    private readonly Mock<IReadRepository<Author>> _authorRepositoryMock = new();
    private readonly ListAuthorsHandler _handler;

    public ListAuthorHandlerTests()
    {
        _handler = new(_authorRepositoryMock.Object);
    }

    [Fact]
    public async Task GivenRequest_ShouldReturnResult_WhenAuthorsIsNotEmpty()
    {
        // Arrange
        var authors = AuthorBuilder.WithDefaultValues();
        _authorRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(authors);

        // Act
        var result = await _handler.Handle(new(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(authors.Count);
        _authorRepositoryMock.Verify(x => x.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
