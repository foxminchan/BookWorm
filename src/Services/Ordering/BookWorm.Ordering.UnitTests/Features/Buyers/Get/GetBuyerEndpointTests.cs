using BookWorm.Constants;
using BookWorm.Ordering.Features.Buyers;
using BookWorm.Ordering.Features.Buyers.Get;
using BookWorm.SharedKernel.Exceptions;
using MediatR;
using Microsoft.FeatureManagement;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Get;

public sealed class GetBuyerEndpointTests
{
    private readonly BuyerDto _buyerDto;
    private readonly GetBuyerEndpoint _endpoint;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly GetBuyerQuery _query;
    private readonly Mock<ISender> _senderMock;

    public GetBuyerEndpointTests()
    {
        _senderMock = new();
        _featureManagerMock = new();
        _endpoint = new();

        var buyerId = Guid.CreateVersion7();
        _buyerDto = new(buyerId, "Test Buyer", "123 Test Street");
        _query = new(buyerId);
    }

    [Test]
    public async Task GivenEnabledAddressFeature_WhenHandlingGetBuyerQuery_ThenShouldReturnBuyerWithAddress()
    {
        // Arrange
        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(nameof(FeatureFlags.EnableAddress)))
            .ReturnsAsync(true);

        _senderMock
            .Setup(s => s.Send(_query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyerDto);

        // Act
        var result = await _endpoint.HandleAsync(
            _query,
            _senderMock.Object,
            _featureManagerMock.Object
        );

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(_buyerDto.Id);
        result.Value.Name.ShouldBe(_buyerDto.Name);
        result.Value.Address.ShouldBe(_buyerDto.Address); // Address should be preserved
    }

    [Test]
    public async Task GivenDisabledAddressFeature_WhenHandlingGetBuyerQuery_ThenShouldReturnBuyerWithNullAddress()
    {
        // Arrange
        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(nameof(FeatureFlags.EnableAddress)))
            .ReturnsAsync(false);

        _senderMock
            .Setup(s => s.Send(_query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyerDto);

        // Act
        var result = await _endpoint.HandleAsync(
            _query,
            _senderMock.Object,
            _featureManagerMock.Object
        );

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(_buyerDto.Id);
        result.Value.Name.ShouldBe(_buyerDto.Name);
        result.Value.Address.ShouldBeNull(); // Address should be null
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingGetBuyerQuery_ThenShouldPropagateException()
    {
        // Arrange
        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(nameof(FeatureFlags.EnableAddress)))
            .ReturnsAsync(true);

        var expectedException = new NotFoundException("Buyer not found");
        _senderMock
            .Setup(s => s.Send(_query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () =>
            await _endpoint.HandleAsync(_query, _senderMock.Object, _featureManagerMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe(expectedException.Message);
    }
}
