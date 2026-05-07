using Wolverine;

namespace BookWorm.Chassis.EventBus.Wolverine;

internal sealed class CloudEventHeaderPolicy : IEnvelopeRule
{
    private readonly string? _source;

    public CloudEventHeaderPolicy() { }

    public CloudEventHeaderPolicy(string source)
    {
        _source = source;
    }

    public void Modify(Envelope envelope)
    {
        if (!string.IsNullOrEmpty(envelope.MessageType))
        {
            envelope.Headers.TryAdd("messagetype", envelope.MessageType);
        }

        if (envelope.Destination is not null)
        {
            envelope.Headers.TryAdd("destinationaddress", envelope.Destination.ToString());
        }

        if (envelope.ReplyUri is not null)
        {
            envelope.Headers.TryAdd("responseaddress", envelope.ReplyUri.ToString());
        }

        if (!string.IsNullOrEmpty(_source))
        {
            envelope.Headers.TryAdd("source", _source);
        }
    }

    public void ApplyCorrelation(IMessageContext originator, Envelope outgoing)
    {
        Modify(outgoing);

        if (originator.Envelope is null)
        {
            return;
        }

        if (
            originator.Envelope.Headers.TryGetValue("requestid", out var requestId)
            && !string.IsNullOrEmpty(requestId)
        )
        {
            outgoing.Headers.TryAdd("requestid", requestId);
        }

        if (
            !originator.Envelope.Headers.TryGetValue(EventBusHeaders.UserId, out var userId)
            || string.IsNullOrEmpty(userId)
        )
        {
            return;
        }

        outgoing.Headers.TryAdd(EventBusHeaders.UserId, userId);
        outgoing.Headers.TryAdd("userid", userId);
    }
}
