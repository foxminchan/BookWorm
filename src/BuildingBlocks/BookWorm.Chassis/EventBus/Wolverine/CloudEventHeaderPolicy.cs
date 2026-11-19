using Wolverine;

namespace BookWorm.Chassis.EventBus.Wolverine;

internal sealed class CloudEventHeaderPolicy(string source) : IEnvelopeRule
{
    private readonly string? _source = source;

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
    }
}
