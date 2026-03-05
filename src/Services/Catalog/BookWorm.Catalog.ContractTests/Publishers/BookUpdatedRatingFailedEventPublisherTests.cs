using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Catalog.ContractTests.Publishers;

public sealed class BookUpdatedRatingFailedEventPublisherTests
{
    private readonly int _rating = 4;
    private Guid _bookId;
    private Guid _feedbackId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<IBookRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _bookId = Guid.CreateVersion7();
        _feedbackId = Guid.CreateVersion7();

        _unitOfWorkMock = new();
        _unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x => x.AddConsumer<FeedbackCreatedIntegrationEventHandler>())
            .AddScoped(_ => _repositoryMock.Object)
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task GivenNonExistingBook_WhenHandlingFeedbackCreated_ThenShouldPublishFailedEvent()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var integrationEvent = new FeedbackCreatedIntegrationEvent(_bookId, _rating, _feedbackId);

        // Act
        await _harness.Bus.Publish(integrationEvent);

        // Assert
        await _harness.Consumed.Any<FeedbackCreatedIntegrationEvent>();

        await SnapshotTestHelper.Verify(_harness);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_bookId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
