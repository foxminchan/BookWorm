using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.Delete;
using BookWorm.Ordering.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Delete;

public sealed class DeleteBuyerCommandTests
{
    private readonly Buyer _buyer;
    private readonly Guid _buyerId;
    private readonly DeleteBuyerHandler _handler;
    private readonly Mock<IBuyerRepository> _repositoryMock;

    public DeleteBuyerCommandTests()
    {
        _buyer = new BuyerFaker().Generate()[0];
        _buyerId = _buyer.Id;

        _repositoryMock = new();
        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenExistingBuyer_WhenHandlingDeleteCommand_ThenShouldCallDelete()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyer);

        // Act
        var result = await _handler.Handle(new(_buyerId), CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(x => x.Delete(_buyer), Times.Once);
    }

    [Test]
    public async Task GivenExistingBuyer_WhenHandlingDeleteCommand_ThenShouldCallSaveEntitiesAsync()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyer);

        // Act
        await _handler.Handle(new(_buyerId), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistingBuyer_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.CreateVersion7();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(nonExistingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Buyer)default!);

        // Act
        var act = async () => await _handler.Handle(new(nonExistingId), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Buyer with id {nonExistingId} not found.");
        _repositoryMock.Verify(x => x.Delete(It.IsAny<Buyer>()), Times.Never);
        _repositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
