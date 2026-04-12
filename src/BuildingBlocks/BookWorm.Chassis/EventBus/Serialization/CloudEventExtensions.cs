using CloudNative.CloudEvents;

namespace BookWorm.Chassis.EventBus.Serialization;

internal static class CloudEventExtensions
{
    internal static readonly CloudEventAttribute CorrelationId =
        CloudEventAttribute.CreateExtension("correlationid", CloudEventAttributeType.String);

    internal static readonly CloudEventAttribute ConversationId =
        CloudEventAttribute.CreateExtension("conversationid", CloudEventAttributeType.String);

    internal static readonly CloudEventAttribute InitiatorId = CloudEventAttribute.CreateExtension(
        "initiatorid",
        CloudEventAttributeType.String
    );

    internal static readonly CloudEventAttribute RequestId = CloudEventAttribute.CreateExtension(
        "requestid",
        CloudEventAttributeType.String
    );

    internal static readonly CloudEventAttribute DestinationAddress =
        CloudEventAttribute.CreateExtension("destinationaddress", CloudEventAttributeType.String);

    internal static readonly CloudEventAttribute ResponseAddress =
        CloudEventAttribute.CreateExtension("responseaddress", CloudEventAttributeType.String);

    internal static readonly CloudEventAttribute FaultAddress = CloudEventAttribute.CreateExtension(
        "faultaddress",
        CloudEventAttributeType.String
    );

    internal static readonly CloudEventAttribute MessageType = CloudEventAttribute.CreateExtension(
        "messagetype",
        CloudEventAttributeType.String
    );

    internal static readonly CloudEventAttribute[] All =
    [
        CorrelationId,
        ConversationId,
        InitiatorId,
        RequestId,
        DestinationAddress,
        ResponseAddress,
        FaultAddress,
        MessageType,
    ];
}
