using Wolverine;

namespace BookWorm.Chassis.EventBus.Wolverine;

/// <summary>
///     Wolverine envelope rule that propagates BookWorm-specific CloudEvent extension
///     attributes between <see cref="Envelope.Headers" /> and the outbound Kafka message.
///     The built-in <c>InteropWithCloudEvents</c> mapper handles the standard attributes
///     (<c>id</c>, <c>type</c>, <c>source</c>, <c>time</c>, <c>datacontenttype</c>,
///     <c>correlationid</c>, <c>conversationid</c>, <c>initiatorid</c>). This rule
///     preserves the MassTransit-era extension attributes required by FR-003 and US2.
/// </summary>
internal sealed class CloudEventHeaderPolicy : IEnvelopeRule
{
    /// <summary>
    ///     Stamps the BookWorm extension attributes into <see cref="Envelope.Headers" />
    ///     when a message is published outside a handler context (e.g. directly from an
    ///     HTTP endpoint). The CloudEvents mapper includes all Headers entries as CE
    ///     extension attributes on the wire.
    /// </summary>
    public void Modify(Envelope envelope)
    {
        // Mirror messagetype: Wolverine sets Envelope.MessageType as the CE "type"
        // attribute; duplicate it into the messagetype extension for parity with the
        // MassTransit wire format.
        if (!string.IsNullOrEmpty(envelope.MessageType))
        {
            envelope.Headers.TryAdd("messagetype", envelope.MessageType);
        }

        // destinationaddress / responseaddress
        if (envelope.Destination != null)
        {
            envelope.Headers.TryAdd("destinationaddress", envelope.Destination.ToString());
        }

        if (envelope.ReplyUri != null)
        {
            envelope.Headers.TryAdd("responseaddress", envelope.ReplyUri.ToString());
        }
    }

    /// <summary>
    ///     When a message is published within a handler context, propagate the
    ///     BookWorm extension attributes from the incoming <paramref name="originator" />
    ///     envelope into the <paramref name="outgoing" /> envelope. This ensures that
    ///     correlation information flows correctly through the saga / handler chain.
    /// </summary>
    public void ApplyCorrelation(IMessageContext originator, Envelope outgoing)
    {
        Modify(outgoing);

        if (originator.Envelope is null)
        {
            return;
        }

        // Propagate requestid from the incoming envelope if available.
        if (
            originator.Envelope.Headers.TryGetValue("requestid", out var requestId)
            && !string.IsNullOrEmpty(requestId)
        )
        {
            outgoing.Headers.TryAdd("requestid", requestId);
        }

        // Propagate userid from the incoming envelope if not already set.
        if (
            originator.Envelope.Headers.TryGetValue(EventBusHeaders.UserId, out var userId)
            && !string.IsNullOrEmpty(userId)
        )
        {
            outgoing.Headers.TryAdd(EventBusHeaders.UserId, userId);
            outgoing.Headers.TryAdd("userid", userId);
        }
    }
}
