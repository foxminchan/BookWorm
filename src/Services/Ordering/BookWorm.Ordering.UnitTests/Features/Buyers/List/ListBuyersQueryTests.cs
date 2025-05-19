using BookWorm.Constants;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate.Specifications;
using BookWorm.Ordering.Features.Buyers.List;
using BookWorm.Ordering.UnitTests.Fakers;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.List;

public sealed class ListBuyersQueryTests
{
    private readonly Mock<IBuyerRepository> _buyerRepositoryMock;
    private readonly Buyer[] _buyers;
    private readonly ListBuyersQueryHandler _handler;

    public ListBuyersQueryTests()
    {
        _buyers = new BuyerFaker().Generate();
        _buyerRepositoryMock = new();
        _handler = new(_buyerRepositoryMock.Object);
    }

    [Test]
    public async Task GivenDefaultParameters_WhenHandlingListBuyersQuery_ThenShouldReturnPagedResult()
    {
        // Arrange
        const int pageIndex = Pagination.DefaultPageIndex;
        const int pageSize = Pagination.DefaultPageSize;
        const int totalItems = 10;

        _buyerRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<BuyerFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyers);

        _buyerRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        var query = new ListBuyersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBe(_buyers.Length);
        result.PageIndex.ShouldBe(pageIndex);
        result.PageSize.ShouldBe(pageSize);
        result.TotalItems.ShouldBe(totalItems);
        result.TotalPages.ShouldBe((int)Math.Ceiling(totalItems / (double)pageSize));

        _buyerRepositoryMock.Verify(
            x =>
                x.ListAsync(
                    It.Is<BuyerFilterSpec>(s =>
                        s.Skip == (pageIndex - 1) * pageSize && s.Take == pageSize
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCustomPagination_WhenHandlingListBuyersQuery_ThenShouldReturnCorrectlyPagedResult()
    {
        // Arrange
        const int pageIndex = 2;
        const int pageSize = 5;
        const int totalItems = 25;

        _buyerRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<BuyerFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyers);

        _buyerRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        var query = new ListBuyersQuery(pageIndex, pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.PageIndex.ShouldBe(pageIndex);
        result.PageSize.ShouldBe(pageSize);
        result.TotalItems.ShouldBe(totalItems);
        result.TotalPages.ShouldBe((int)Math.Ceiling(totalItems / (double)pageSize)); // Should be 5

        _buyerRepositoryMock.Verify(
            x =>
                x.ListAsync(
                    It.Is<BuyerFilterSpec>(s =>
                        s.Skip == (pageIndex - 1) * pageSize && s.Take == pageSize
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyRepository_WhenHandlingListBuyersQuery_ThenShouldReturnEmptyPagedResult()
    {
        // Arrange
        const int totalItems = 0;

        _buyerRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<BuyerFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _buyerRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        var query = new ListBuyersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalItems.ShouldBe(totalItems);
        result.TotalPages.ShouldBe(0);
    }

    [Test]
    [Arguments(0)] // Edge case: page index of 0
    [Arguments(-1)] // Edge case: negative page index
    public async Task GivenInvalidPageIndex_WhenHandlingListBuyersQuery_ThenShouldStillExecuteQuery(
        int pageIndex
    )
    {
        // Arrange
        const int pageSize = Pagination.DefaultPageSize;

        _buyerRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<BuyerFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyers);

        _buyerRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var query = new ListBuyersQuery(pageIndex);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.PageIndex.ShouldBe(pageIndex);

        // Verify repository was still called with the provided parameters
        _buyerRepositoryMock.Verify(
            x =>
                x.ListAsync(
                    It.Is<BuyerFilterSpec>(s =>
                        s.Skip == (pageIndex - 1) * pageSize && s.Take == pageSize
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationRequested_WhenHandlingListBuyersQuery_ThenShouldPropagateToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);
        var query = new ListBuyersQuery();

        // Since we're using a canceled token, the operation should throw when the token is used
        _buyerRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<BuyerFilterSpec>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _handler.Handle(query, cancellationToken)
        );

        _buyerRepositoryMock.Verify(
            x => x.ListAsync(It.IsAny<BuyerFilterSpec>(), cancellationToken),
            Times.Once
        );
    }
}
