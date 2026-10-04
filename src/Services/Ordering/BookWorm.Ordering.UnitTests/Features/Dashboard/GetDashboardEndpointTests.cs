using BookWorm.Ordering.Features.Dashboard;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Dashboard;

public sealed class GetDashboardEndpointTests
{
    private readonly GetDashboardEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    private readonly DashboardDto _snapshot = new(
        40,
        250,
        30,
        35,
        [],
        [],
        [],
        new(2026, 10, 4, 0, 0, 0, TimeSpan.Zero)
    );

    [Test]
    public async Task GivenSnapshot_WhenHandlingDashboardRequest_ThenShouldReturnDashboard()
    {
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(_snapshot);

        var result = await _endpoint.HandleAsync(_senderMock.Object);

        result.ShouldBeOfType<Ok<DashboardDto>>();
        result.Value.ShouldBe(_snapshot);
        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingDashboardRequest_ThenShouldPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;
        _senderMock
            .Setup(sender => sender.Send(It.IsAny<GetDashboardQuery>(), cancellationToken))
            .ReturnsAsync(_snapshot);

        await _endpoint.HandleAsync(_senderMock.Object, cancellationToken);

        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenQueryFailure_WhenHandlingDashboardRequest_ThenShouldPropagateException()
    {
        var exception = new InvalidOperationException("Dashboard unavailable.");
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(exception);

        var result = await Should.ThrowAsync<InvalidOperationException>(() =>
            _endpoint.HandleAsync(_senderMock.Object)
        );

        result.ShouldBe(exception);
    }
}
