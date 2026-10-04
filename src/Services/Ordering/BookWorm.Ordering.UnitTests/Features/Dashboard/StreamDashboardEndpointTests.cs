using BookWorm.Ordering.Features.Dashboard;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.UnitTests.Features.Dashboard;

public sealed class StreamDashboardEndpointTests
{
    private readonly StreamDashboardEndpoint _endpoint = new();
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
    public async Task GivenSnapshot_WhenHandlingStreamRequest_ThenShouldReturnNativeSseResultAndPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;
        _senderMock
            .Setup(sender => sender.Send(It.IsAny<GetDashboardQuery>(), cancellationToken))
            .ReturnsAsync(_snapshot);

        var result = await _endpoint.HandleAsync(_senderMock.Object, new(), cancellationToken);

        result.ShouldBeOfType<ServerSentEventsResult<DashboardDto>>();
        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenInitialQueryFailure_WhenHandlingStreamRequest_ThenShouldFailBeforeStartingStream()
    {
        var exception = new InvalidOperationException("Dashboard unavailable.");
        _senderMock
            .Setup(sender =>
                sender.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(exception);

        var result = await Should.ThrowAsync<InvalidOperationException>(() =>
            _endpoint.HandleAsync(_senderMock.Object, new())
        );

        result.ShouldBe(exception);
        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSnapshot_WhenStreaming_ThenShouldSendImmediately()
    {
        await using var stream = StreamDashboardEndpoint
            .StreamAsync(_senderMock.Object, _snapshot, CancellationToken.None)
            .GetAsyncEnumerator();

        var hasSnapshot = await stream.MoveNextAsync();

        hasSnapshot.ShouldBeTrue();
        stream.Current.ShouldBe(_snapshot);
        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenClientDisconnect_WhenStreaming_ThenShouldStopWithoutFetchingAnotherSnapshot()
    {
        using var cancellation = new CancellationTokenSource();
        await using var stream = StreamDashboardEndpoint
            .StreamAsync(_senderMock.Object, _snapshot, cancellation.Token)
            .GetAsyncEnumerator(cancellation.Token);
        await stream.MoveNextAsync();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await stream.MoveNextAsync()
        );

        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenCanceledRequest_WhenStartingStream_ThenShouldNotSendSnapshot()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await using var stream = StreamDashboardEndpoint
            .StreamAsync(_senderMock.Object, _snapshot, cancellation.Token)
            .GetAsyncEnumerator(cancellation.Token);

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await stream.MoveNextAsync()
        );

        _senderMock.Verify(
            sender => sender.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
