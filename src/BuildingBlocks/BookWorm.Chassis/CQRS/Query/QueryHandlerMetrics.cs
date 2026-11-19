using System.Diagnostics;
using System.Diagnostics.Metrics;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;

namespace BookWorm.Chassis.CQRS.Query;

internal sealed class QueryHandlerMetrics : IDisposable
{
    private readonly UpDownCounter<long> _activeEventHandlingCounter;
    private readonly QueryHandlingDuration _eventHandlingDuration;
    private readonly Meter _meter;
    private readonly TimeProvider _timeProvider;
    private readonly TotalQueriesNumber _totalQueriesNumber;

    public QueryHandlerMetrics(IMeterFactory meterFactory, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        _totalQueriesNumber = QueryHandlerMetricsExtensions.CreateTotalQueriesNumber(_meter);

        _activeEventHandlingCounter = _meter.CreateUpDownCounter<long>(
            TelemetryTags.Queries.ActiveQueriesNumber,
            "{query}",
            "Number of queries currently being handled"
        );

        _eventHandlingDuration = QueryHandlerMetricsExtensions.CreateQueryHandlingDuration(_meter);
    }

    public void Dispose()
    {
        _meter.Dispose();
    }

    public long QueryHandlingStart(string queryType)
    {
        if (_activeEventHandlingCounter.Enabled)
        {
            var tags = new TagList { { TelemetryTags.Queries.QueryType, queryType } };
            _activeEventHandlingCounter.Add(1, tags);
        }

        _totalQueriesNumber.Add(1, queryType);

        return _timeProvider.GetTimestamp();
    }

    public void QueryHandlingEnd(string queryType, long startingTimestamp)
    {
        if (_activeEventHandlingCounter.Enabled)
        {
            var tags = new TagList { { TelemetryTags.Queries.QueryType, queryType } };
            _activeEventHandlingCounter.Add(-1, tags);
        }

        var elapsed = _timeProvider.GetElapsedTime(startingTimestamp);

        _eventHandlingDuration.Record(elapsed.TotalSeconds, queryType);
    }
}
