using BookWorm.Catalog.Features.Publishers.List;
using BookWorm.Catalog.UnitTests.Builder;

namespace BookWorm.Catalog.UnitTests.Application.Publishers;

public sealed class ListPublishersHandlerTests
{
    private readonly ListPublishersHandler _handler;
    private readonly Mock<IReadRepository<Publisher>> _publisherRepositoryMock;

    public ListPublishersHandlerTests()
    {
        _publisherRepositoryMock = new();
        _handler = new(_publisherRepositoryMock.Object);
    }

    [Fact]
    public async Task GivenRequest_ShouldReturnResult_WhenPublishersIsNotEmpty()
    {
        // Arrange
        var publishers = PublisherBuilder.WithDefaultValues();

        _publisherRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(publishers);

        var query = new ListPublishersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Should().BeEquivalentTo(publishers);
        _publisherRepositoryMock.Verify(
            x => x.ListAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
