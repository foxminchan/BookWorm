using BookWorm.Chassis.OpenTelemetry;

namespace BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

public static class DocumentSessionOpenTelemetryExtensions
{
    extension(IDocumentSession documentSession)
    {
        public void PropagateTelemetry(Activity? activity, ILogger? logger = null)
        {
            var propagationContext = activity.Propagate(
                documentSession,
                (session, key, value) =>
                    session.InjectTelemetryIntoDocumentSession(key, value, logger)
            );

            if (!propagationContext.HasValue)
            {
                return;
            }

            documentSession.CorrelationId =
                propagationContext.Value.ActivityContext.TraceId.ToHexString();
            documentSession.CausationId =
                propagationContext.Value.ActivityContext.SpanId.ToHexString();
        }

        private void InjectTelemetryIntoDocumentSession(
            string key,
            string value,
            ILogger? logger = null
        )
        {
            try
            {
                documentSession.SetHeader(key, value);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to inject trace context");
            }
        }
    }
}
