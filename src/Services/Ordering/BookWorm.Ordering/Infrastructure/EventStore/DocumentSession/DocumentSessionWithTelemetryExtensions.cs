﻿using System.Runtime.CompilerServices;

namespace BookWorm.Ordering.Infrastructure.EventStore.DocumentSession;

public static class DocumentSessionWithTelemetryExtensions
{
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
