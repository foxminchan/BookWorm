using System.Runtime.CompilerServices;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

namespace BookWorm.Ordering.Infrastructure.EventStore.Extensions;

internal static class DocumentSessionWithTelemetryExtensions
{
    public static async Task Add<T>(
        this IDocumentSession documentSession,
        Guid id,
        DomainEvent @event,
        CancellationToken ct = default
    )
        where T : class
    {
        await documentSession.WithTelemetry<T>(
            async token =>
            {
                documentSession.Events.StartStream<T>(id, @event);
                await documentSession.SaveChangesAsync(token);
            },
            ct
        );
    }

    public static Task Add<T>(
        this IDocumentSession documentSession,
        Guid id,
        IntegrationEvent @event,
        CancellationToken ct = default
    )
        where T : class
    {
        return documentSession.WithTelemetry<IntegrationEvent>(
            async token =>
            {
                documentSession.Events.StartStream<T>(id, @event);
                await documentSession.SaveChangesAsync(token);
            },
            ct
        );
    }

    public static async Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        DomainEvent @event,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        await documentSession.WithTelemetry<T>(
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

    public static async Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        IntegrationEvent @event,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        await documentSession.WithTelemetry<IntegrationEvent>(
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

    private static async Task WithTelemetry<T>(
        this IDocumentSession documentSession,
        Func<CancellationToken, Task> run,
        CancellationToken cancellationToken,
        [CallerMemberName] string memberName = ""
    )
    {
        await ActivityScope.Instance.Run(
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
