using BookWorm.SharedKernel.OpenTelemetry;

namespace BookWorm.Ordering.Infrastructure.EventStore.DocumentSession;

public static class DocumentSessionOpenTelemetryExtensions
{
    public static void PropagateTelemetry(
        this IDocumentSession documentSession,
        Activity? activity,
        ILogger? logger = null
    )
    {
        var propagationContext = activity.Propagate(
            documentSession,
            (session, key, value) => session.InjectTelemetryIntoDocumentSession(key, value, logger)
        );

        if (!propagationContext.HasValue)
        {
            return;
        }

        documentSession.CorrelationId =
            propagationContext.Value.ActivityContext.TraceId.ToHexString();
        documentSession.CausationId = propagationContext.Value.ActivityContext.SpanId.ToHexString();
    }

    private static void InjectTelemetryIntoDocumentSession(
        this IDocumentSession session,
        string key,
        string value,
        ILogger? logger = null
    )
    {
        try
        {
            session.SetHeader(key, value);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to inject trace context");
        }
    }
}
