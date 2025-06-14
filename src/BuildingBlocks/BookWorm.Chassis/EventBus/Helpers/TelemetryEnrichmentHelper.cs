using System.Diagnostics;
using System.Globalization;
using BookWorm.Chassis.OpenTelemetry;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Helpers;

internal static class TelemetryEnrichmentHelper
{
    public static void EnrichWithCommonTags<T>(
        Activity? activity,
        ConsumeContext<T> context,
        string operation
    )
        where T : class
    {
        var telemetryData = new MessageTelemetryData(
            operation,
            context.MessageId,
            context.ConversationId,
            context.CorrelationId,
            context.Message.GetType(),
            context.DestinationAddress,
            context.SourceAddress,
            context.RequestId
        );

        EnrichWithBasicTags(activity, telemetryData);
    }

    public static void EnrichWithCommonTags<T>(
        Activity? activity,
        PublishContext<T> context,
        string operation
    )
        where T : class
    {
        var telemetryData = new MessageTelemetryData(
            operation,
            context.MessageId,
            context.ConversationId,
            context.CorrelationId,
            context.Message.GetType(),
            context.DestinationAddress,
            context.SourceAddress,
            context.RequestId
        );

        EnrichWithBasicTags(activity, telemetryData);
    }

    public static void EnrichWithCommonTags<T>(
        Activity? activity,
        SendContext<T> context,
        string operation
    )
        where T : class
    {
        var telemetryData = new MessageTelemetryData(
            operation,
            context.MessageId,
            context.ConversationId,
            context.CorrelationId,
            context.Message.GetType(),
            context.DestinationAddress,
            context.SourceAddress,
            context.RequestId
        );

        EnrichWithBasicTags(activity, telemetryData);
    }

    private static void EnrichWithBasicTags(Activity? activity, MessageTelemetryData data)
    {
        activity?.SetTag(TelemetryTags.Messaging.System, "rabbitmq");
        activity?.SetTag(TelemetryTags.Messaging.Operation, data.Operation);
        activity?.SetTag(TelemetryTags.Messaging.MessageId, data.MessageId?.ToString());
        activity?.SetTag(TelemetryTags.Messaging.ConversationId, data.ConversationId?.ToString());
        activity?.SetTag(TelemetryTags.Messaging.CorrelationId, data.CorrelationId?.ToString());
        activity?.SetTag(TelemetryTags.Messaging.MessageType, data.MessageType.FullName);
        activity?.SetTag(TelemetryTags.Messaging.Destination, data.DestinationAddress?.ToString());
        activity?.SetTag(TelemetryTags.Messaging.SourceAddress, data.SourceAddress?.ToString());

        if (data.RequestId.HasValue)
        {
            activity?.SetTag(TelemetryTags.Messaging.RequestId, data.RequestId.ToString());
        }
    }

    public static void EnrichWithConsumeSpecificTags(Activity? activity, ConsumeContext context)
    {
        activity?.SetTag(
            TelemetryTags.Messaging.Consumer,
            context.ReceiveContext.InputAddress.ToString()
        );

        if (context.SentTime.HasValue)
        {
            activity?.SetTag(
                TelemetryTags.Messaging.SentTime,
                context.SentTime.Value.ToString("O")
            );
        }
    }

    public static void EnrichWithSendSpecificTags<T>(Activity? activity, SendContext<T> context)
        where T : class
    {
        if (context.TimeToLive.HasValue)
        {
            activity?.SetTag(
                TelemetryTags.Messaging.TimeToLive,
                context.TimeToLive.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture)
            );
        }

        if (context.Delay.HasValue)
        {
            activity?.SetTag(
                TelemetryTags.Messaging.Delay,
                context.Delay.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture)
            );
        }
    }

    public static string GetActivityName<T>(string operation)
    {
        return $"MassTransit.{operation}.{typeof(T).Name}";
    }
}
