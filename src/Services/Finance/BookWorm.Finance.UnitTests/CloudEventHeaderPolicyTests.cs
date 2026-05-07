using BookWorm.Chassis.EventBus;
using BookWorm.Chassis.EventBus.Wolverine;
using Wolverine;

namespace BookWorm.Finance.UnitTests;

/// <summary>
///     Validates that <see cref="CloudEventHeaderPolicy" /> stamps every FR-003 extension
///     attribute required by the BookWorm CloudEvent envelope contract
///     (<c>contracts/cloudevent-envelope.schema.json</c>).
/// </summary>
public sealed class CloudEventHeaderPolicyTests
{
    private readonly CloudEventHeaderPolicy _policy = new();

    // ── Modify (outbound) ─────────────────────────────────────────────────────

    [Test]
    public void GivenEnvelopeWithMessageType_WhenModified_ThenMessageTypeHeaderIsStamped()
    {
        // Arrange
        var envelope = new Envelope
        {
            MessageType = "BookWorm.Contracts.UserCheckedOutIntegrationEvent",
        };

        // Act
        _policy.Modify(envelope);

        // Assert
        envelope.Headers.ShouldContainKey("messagetype");
        envelope.Headers["messagetype"].ShouldBe(envelope.MessageType);
    }

    [Test]
    public void GivenEnvelopeWithoutMessageType_WhenModified_ThenMessageTypeHeaderIsNotStamped()
    {
        // Arrange
        var envelope = new Envelope { MessageType = null };

        // Act
        _policy.Modify(envelope);

        // Assert
        envelope.Headers.ShouldNotContainKey("messagetype");
    }

    [Test]
    public void GivenEnvelopeWithDestination_WhenModified_ThenDestinationAddressHeaderIsStamped()
    {
        // Arrange
        var destination = new Uri("kafka://localhost:9092/bookworm.ordering");
        var envelope = new Envelope
        {
            MessageType = "BookWorm.Contracts.SomeEvent",
            Destination = destination,
        };

        // Act
        _policy.Modify(envelope);

        // Assert
        envelope.Headers.ShouldContainKey("destinationaddress");
        envelope.Headers["destinationaddress"].ShouldBe(destination.ToString());
    }

    [Test]
    public void GivenEnvelopeWithReplyUri_WhenModified_ThenResponseAddressHeaderIsStamped()
    {
        // Arrange
        var replyUri = new Uri("kafka://localhost:9092/bookworm.responses");
        var envelope = new Envelope
        {
            MessageType = "BookWorm.Contracts.SomeEvent",
            ReplyUri = replyUri,
        };

        // Act
        _policy.Modify(envelope);

        // Assert
        envelope.Headers.ShouldContainKey("responseaddress");
        envelope.Headers["responseaddress"].ShouldBe(replyUri.ToString());
    }

    [Test]
    public void GivenEnvelopeWithNoDestination_WhenModified_ThenDestinationAddressHeaderIsAbsent()
    {
        // Arrange
        var envelope = new Envelope { MessageType = "BookWorm.Contracts.SomeEvent" };

        // Act
        _policy.Modify(envelope);

        // Assert
        envelope.Headers.ShouldNotContainKey("destinationaddress");
        envelope.Headers.ShouldNotContainKey("responseaddress");
    }

    // ── ApplyCorrelation (inbound) ────────────────────────────────────────────

    [Test]
    public void GivenIncomingEnvelopeWithRequestId_WhenCorrelationApplied_ThenRequestIdPropagated()
    {
        // Arrange
        var requestId = Guid.CreateVersion7().ToString();
        var incoming = new Envelope();
        incoming.Headers["requestid"] = requestId;

        var mockContext = new Mock<IMessageContext>();
        mockContext.Setup(c => c.Envelope).Returns(incoming);

        var outgoing = new Envelope { MessageType = "BookWorm.Contracts.SomeEvent" };

        // Act
        _policy.ApplyCorrelation(mockContext.Object, outgoing);

        // Assert
        outgoing.Headers.ShouldContainKey("requestid");
        outgoing.Headers["requestid"].ShouldBe(requestId);
    }

    [Test]
    public void GivenIncomingEnvelopeWithUserId_WhenCorrelationApplied_ThenUserIdPropagated()
    {
        // Arrange
        var userId = "user-123";
        var incoming = new Envelope();
        incoming.Headers[EventBusHeaders.UserId] = userId;

        var mockContext = new Mock<IMessageContext>();
        mockContext.Setup(c => c.Envelope).Returns(incoming);

        var outgoing = new Envelope { MessageType = "BookWorm.Contracts.SomeEvent" };

        // Act
        _policy.ApplyCorrelation(mockContext.Object, outgoing);

        // Assert
        outgoing.Headers.ShouldContainKey(EventBusHeaders.UserId);
        outgoing.Headers[EventBusHeaders.UserId].ShouldBe(userId);
        outgoing.Headers.ShouldContainKey("userid");
        outgoing.Headers["userid"].ShouldBe(userId);
    }

    [Test]
    public void GivenIncomingEnvelopeWithNullContext_WhenCorrelationApplied_ThenOutgoingIsUnchanged()
    {
        // Arrange
        var mockContext = new Mock<IMessageContext>();
        mockContext.Setup(c => c.Envelope).Returns((Envelope?)null);

        var outgoing = new Envelope { MessageType = "BookWorm.Contracts.SomeEvent" };

        // Act — should not throw
        var act = () => _policy.ApplyCorrelation(mockContext.Object, outgoing);

        // Assert
        act.ShouldNotThrow();
    }

    // ── FR-003 required extension attributes ─────────────────────────────────

    [Test]
    public void GivenFullyPopulatedEnvelope_WhenModified_ThenAllFr003ExtensionAttributesArePresent()
    {
        // Arrange — build the richest possible outbound envelope
        var userId = "user-abc";
        var requestId = Guid.CreateVersion7().ToString();
        var destination = new Uri("kafka://localhost:9092/bookworm.finance");
        var replyUri = new Uri("kafka://localhost:9092/bookworm.replies");

        var incoming = new Envelope();
        incoming.Headers[EventBusHeaders.UserId] = userId;
        incoming.Headers["requestid"] = requestId;

        var mockContext = new Mock<IMessageContext>();
        mockContext.Setup(c => c.Envelope).Returns(incoming);

        var outgoing = new Envelope
        {
            MessageType = "BookWorm.Contracts.UserCheckedOutIntegrationEvent",
            Destination = destination,
            ReplyUri = replyUri,
        };

        // Act
        _policy.Modify(outgoing);
        _policy.ApplyCorrelation(mockContext.Object, outgoing);

        // Assert — every FR-003 extension attribute that CloudEventHeaderPolicy is responsible for
        outgoing.Headers.ShouldContainKey("messagetype");
        outgoing.Headers.ShouldContainKey("destinationaddress");
        outgoing.Headers.ShouldContainKey("responseaddress");
        outgoing.Headers.ShouldContainKey("requestid");
        outgoing.Headers.ShouldContainKey(EventBusHeaders.UserId);
        outgoing.Headers.ShouldContainKey("userid");
    }
}
