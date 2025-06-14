namespace BookWorm.Chassis.EventBus.Helpers;

internal sealed record MessageTelemetryData(
    string Operation,
    Guid? MessageId,
    Guid? ConversationId,
    Guid? CorrelationId,
    Type MessageType,
    Uri? DestinationAddress,
    Uri? SourceAddress,
    Guid? RequestId
);
