using System.Security.Claims;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using BookWorm.Ordering.Infrastructure.Helpers;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Create;

public sealed class CreateOrderCommandTests
{
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly CreateOrderCommand _command;
    private readonly CreateOrderHandler _handler;
    private readonly Mock<IDistributedAccessLockProvider> _lockProviderMock;
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly string _userId;

    public CreateOrderCommandTests()
    {
        _userId = Guid.CreateVersion7().ToString();
        _repositoryMock = new();
        _claimsPrincipalMock = new();
        _lockProviderMock = new();

        var claim = new Claim(ClaimTypes.NameIdentifier, _userId);
        _claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

        _handler = new(
            _repositoryMock.Object,
            _claimsPrincipalMock.Object,
            _lockProviderMock.Object
        );

        _command = new()
        {
            Items = [new(Guid.CreateVersion7(), 2, 10.99m), new(Guid.CreateVersion7(), 1, 15.50m)],
        };
    }

    [Test]
    public void GivenCreateOrderCommand_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Act
        var command = new CreateOrderCommand();

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<CreateOrderCommand>();
        command.ShouldBeAssignableTo<ICommand<Guid>>();
    }

    [Test]
    public async Task GivenUnauthenticatedUser_WhenHandlingCreateOrder_ThenShouldThrowUnauthorizedException()
    {
        // Arrange
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim?)default!);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenOrderIsAlreadyLocked_ThenShouldThrowConcurrencyException()
    {
        // Arrange
        var expectedOrder = new Order(_userId.ToBuyerId(), null, _command.Items);
        var mockLock = new Mock<IDistributedAccessLock>();
        mockLock.Setup(x => x.IsAcquired).Returns(false);

        _lockProviderMock
            .Setup(x =>
                x.TryAcquireAsync(
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(mockLock.Object);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Other process is already creating an order");
    }

    [Test]
    public async Task GivenValidCommand_WhenCreatingOrder_ThenShouldReturnOrderId()
    {
        // Arrange
        var expectedOrder = new Order(_userId.ToBuyerId(), null, _command.Items);
        var mockLock = new Mock<IDistributedAccessLock>();
        mockLock.Setup(x => x.IsAcquired).Returns(true);
        mockLock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

        _lockProviderMock
            .Setup(x =>
                x.TryAcquireAsync(
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(mockLock.Object);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedOrder.Id);
        _repositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<Order>(o =>
                        o.BuyerId == _userId.ToBuyerId()
                        && o.OrderItems.Count == _command.Items.Count
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        mockLock.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
