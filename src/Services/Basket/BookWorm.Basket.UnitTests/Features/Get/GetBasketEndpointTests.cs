using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Basket.UnitTests.Features.Get;

public sealed class GetBasketEndpointTests
{
    private readonly CustomerBasketDto _customerBasketDto = new(
        Guid.CreateVersion7().ToString(),
        [
            new("book-1", 2)
            {
                Name = "Test Book",
                Price = 19.99m,
                PriceSale = 15.99m,
            },
        ]
    );

    private readonly GetBasketEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidRequest_WhenHandlingGetBasket_ThenShouldReturnOkWithBasket()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBasketQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_customerBasketDto);

        // Act
        var result = await _endpoint.HandleAsync(_senderMock.Object, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<CustomerBasketDto>>();
        result.Value.ShouldBe(_customerBasketDto);
        _senderMock.Verify(
            x => x.Send(It.IsAny<GetBasketQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingGetBasket_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBasketQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () =>
            await _endpoint.HandleAsync(_senderMock.Object, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Test exception");
    }
}
