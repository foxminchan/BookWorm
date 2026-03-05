using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.IntegrationEvents.EventHandlers;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Repository;
using BookWorm.Common;
using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Catalog.ContractTests.Consumers;

public sealed class FeedbackCreatedConsumerTests
{
    private readonly int _rating = 4;
    private Book _book = null!;
    private Guid _feedbackId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<IBookRepository> _repositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _feedbackId = Guid.CreateVersion7();
        _book = new BookFaker().Generate(1)[0];

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
    public async Task GivenFeedbackCreatedEvent_WhenPublished_ThenConsumerShouldConsumeItAndUpdateBookRating()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_book);

        var integrationEvent = new FeedbackCreatedIntegrationEvent(_book.Id, _rating, _feedbackId);

        // Act
        await _harness.Bus.Publish(integrationEvent);

        // Assert
        var consumer = _harness.GetConsumerHarness<FeedbackCreatedIntegrationEventHandler>();
        await consumer.Consumed.Any<FeedbackCreatedIntegrationEvent>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(_book.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
