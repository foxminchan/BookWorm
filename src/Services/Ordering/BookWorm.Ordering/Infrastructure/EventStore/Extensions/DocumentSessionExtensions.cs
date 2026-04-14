using System.Runtime.CompilerServices;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

namespace BookWorm.Ordering.Infrastructure.EventStore.Extensions;

internal static class DocumentSessionWithTelemetryExtensions
{
    extension(IDocumentSession documentSession)
    {
        public async Task Add<T>(
            Guid id,
            DomainEvent @event,
            IActivityScope activityScope,
            CancellationToken ct = default
        )
            where T : class
        {
            await documentSession.WithTelemetry<T>(
                activityScope,
                async token =>
                {
                    documentSession.Events.StartStream<T>(id, @event);
                    await documentSession.SaveChangesAsync(token);
                },
                ct
            );
        }

        public Task Add<T>(
            Guid id,
            IntegrationEvent @event,
            IActivityScope activityScope,
            CancellationToken ct = default
        )
            where T : class
        {
            return documentSession.WithTelemetry<IntegrationEvent>(
                activityScope,
                async token =>
                {
                    documentSession.Events.StartStream<T>(id, @event);
                    await documentSession.SaveChangesAsync(token);
                },
                ct
            );
        }

        public async Task GetAndUpdate<T>(
            Guid id,
            DomainEvent @event,
            IActivityScope activityScope,
            CancellationToken cancellationToken = default
        )
            where T : class
        {
            await documentSession.WithTelemetry<T>(
                activityScope,
                async token =>
                {
                    await documentSession.Events.WriteToAggregate<T>(
                        id,
                        stream => stream.AppendOne(@event),
                        token
                    );
                    await documentSession.SaveChangesAsync(token);
                },
                cancellationToken
            );
        }

        public async Task GetAndUpdate<T>(
            Guid id,
            IntegrationEvent @event,
            IActivityScope activityScope,
            CancellationToken cancellationToken = default
        )
            where T : class
        {
            await documentSession.WithTelemetry<IntegrationEvent>(
                activityScope,
                async token =>
                {
                    await documentSession.Events.WriteToAggregate<T>(
                        id,
                        stream => stream.AppendOne(@event),
                        token
                    );
                    await documentSession.SaveChangesAsync(token);
                },
                cancellationToken
            );
        }

        private async Task WithTelemetry<T>(
            IActivityScope activityScope,
            Func<CancellationToken, Task> run,
            CancellationToken cancellationToken,
            [CallerMemberName] string memberName = ""
        )
        {
            await activityScope.Run(
                $"{nameof(DocumentSessionWithTelemetryExtensions)}/{memberName}",
                (activity, token) =>
                {
                    documentSession.PropagateTelemetry(activity);

                    return run(token);
                },
                new() { Tags = { { TelemetryTags.Stream, typeof(T).Name } } },
                cancellationToken
            );
        }
    }
}
