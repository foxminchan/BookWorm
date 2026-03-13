namespace BookWorm.Chassis.EventBus.Serialization;

/// <summary>
///     Maps MassTransit message type URNs to/from CloudEvent <c>type</c> attribute values.
/// </summary>
internal static class CloudEventTypes
{
    private const string Prefix = "urn:message:";

    internal static string FromMessageTypes(string[] messageTypes)
    {
        if (messageTypes.Length == 0)
        {
            return "com.masstransit.message";
        }

        // MassTransit URNs look like "urn:message:Namespace:TypeName"
        // Convert to a dot-separated CloudEvent type, e.g. "Namespace.TypeName"
        var primary = messageTypes[0];
        return primary.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            ? primary[Prefix.Length..].Replace(':', '.')
            : primary;
    }
}
