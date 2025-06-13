using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders;
using BookWorm.Ordering.Features.Orders.List;
using BookWorm.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Orders.List;

public sealed class ListOrdersEndpointsTests
{
    private readonly ListOrdersEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidQuery_WhenHandlingRequest_ThenShouldReturnOkWithPagedResult()
    {
        // Arrange
        var query = new ListOrdersQuery(1, 10);
        var expectedResult = new PagedResult<OrderDto>(
            [new(Guid.CreateVersion7(), DateTime.UtcNow, 100.0m, Status.New)],
            1,
            10,
            1
        );

        _senderMock
            .Setup(x => x.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<OrderDto>>>();
        result.Value.ShouldBe(expectedResult);
        _senderMock.Verify(s => s.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCustomPageParameters_WhenHandlingRequest_ThenShouldPassCorrectParametersToSender()
    {
        // Arrange
        var pageIndex = 3;
        var pageSize = 25;
        var query = new ListOrdersQuery(pageIndex, pageSize);

        var expectedResult = new PagedResult<OrderDto>([], pageIndex, pageSize, 0);

        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<ListOrdersQuery>(q => q.PageIndex == pageIndex && q.PageSize == pageSize),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<OrderDto>>>();
        result.Value.ShouldBe(expectedResult);
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListOrdersQuery>(q => q.PageIndex == pageIndex && q.PageSize == pageSize),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenStatusFilter_WhenHandlingRequest_ThenShouldPassFilterToSender()
    {
        // Arrange
        var status = Status.Completed;
        var query = new ListOrdersQuery(1, 10, status);

        var expectedResult = new PagedResult<OrderDto>([], 1, 10, 0);

        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<ListOrdersQuery>(q => q.Status == status),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<OrderDto>>>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListOrdersQuery>(q => q.Status == status),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenBuyerIdFilter_WhenHandlingRequest_ThenShouldPassFilterToSender()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        var query = new ListOrdersQuery(1, 10, null, buyerId);

        var expectedResult = new PagedResult<OrderDto>([], 1, 10, 0);

        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<ListOrdersQuery>(q => q.BuyerId == buyerId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<OrderDto>>>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListOrdersQuery>(q => q.BuyerId == buyerId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenAllFilters_WhenHandlingRequest_ThenShouldPassAllFiltersToSender()
    {
        // Arrange
        var pageIndex = 2;
        var pageSize = 15;
        var status = Status.Completed;
        var buyerId = Guid.CreateVersion7();

        var query = new ListOrdersQuery(pageIndex, pageSize, status, buyerId);

        var expectedResult = new PagedResult<OrderDto>([], pageIndex, pageSize, 0);

        _senderMock
            .Setup(x =>
                x.Send(
                    It.Is<ListOrdersQuery>(q =>
                        q.PageIndex == pageIndex
                        && q.PageSize == pageSize
                        && q.Status == status
                        && q.BuyerId == buyerId
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<PagedResult<OrderDto>>>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<ListOrdersQuery>(q =>
                        q.PageIndex == pageIndex
                        && q.PageSize == pageSize
                        && q.Status == status
                        && q.BuyerId == buyerId
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
