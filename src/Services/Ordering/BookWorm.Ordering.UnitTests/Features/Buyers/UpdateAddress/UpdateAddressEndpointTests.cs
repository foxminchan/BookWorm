using BookWorm.Ordering.Features.Buyers;
using BookWorm.Ordering.Features.Buyers.UpdateAddress;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.UpdateAddress;

public sealed class UpdateAddressEndpointTests
{
    private readonly BuyerDto _buyerDto;
    private readonly UpdateAddressEndpoint _endpoint;
    private readonly Mock<ISender> _senderMock;
    private readonly UpdateAddressCommand _validCommand;

    public UpdateAddressEndpointTests()
    {
        _senderMock = new();
        _endpoint = new();

        // Create a valid command
        _validCommand = new("123 Main St", "Test City", "Test Province");

        // Create a sample buyer DTO for the response
        _buyerDto = new(
            Guid.CreateVersion7(),
            "Test Buyer",
            $"{_validCommand.Street}, {_validCommand.City}, {_validCommand.Province}"
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandleAsync_ThenShouldReturnOkResultWithBuyerDto()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(_validCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyerDto);

        // Act
        var result = await _endpoint.HandleAsync(_validCommand, _senderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<BuyerDto>>();
        result.Value.ShouldBe(_buyerDto);
        _senderMock.Verify(s => s.Send(_validCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandleAsync_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);
        _senderMock.Setup(s => s.Send(_validCommand, cancellationToken)).ReturnsAsync(_buyerDto);

        // Act
        await _endpoint.HandleAsync(_validCommand, _senderMock.Object, cancellationToken);

        // Assert
        _senderMock.Verify(s => s.Send(_validCommand, cancellationToken), Times.Once);
    }
}
