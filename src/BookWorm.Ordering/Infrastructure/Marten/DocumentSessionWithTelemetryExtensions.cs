using System.Runtime.CompilerServices;
using BookWorm.Shared.OpenTelemetry;

namespace BookWorm.Ordering.Infrastructure.Marten;

public static class DocumentSessionWithTelemetryExtensions
{
    public static async Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        EventBase @event,
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
