using System.Diagnostics;
using System.Diagnostics.Metrics;
using BookWorm.Chassis.ActivityScope;
using BookWorm.Chassis.OpenTelemetry;

namespace BookWorm.Chassis.Query;

public sealed class QueryHandlerMetrics : IDisposable
{
    private readonly UpDownCounter<long> _activeEventHandlingCounter;
    private readonly Histogram<double> _eventHandlingDuration;
    private readonly Meter _meter;
    private readonly TimeProvider _timeProvider;
    private readonly Counter<long> _totalQueriesNumber;

    public QueryHandlerMetrics(IMeterFactory meterFactory, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        _totalQueriesNumber = _meter.CreateCounter<long>(
            TelemetryTags.Queries.TotalQueriesNumber,
            "{query}",
            "Total number of queries send to query handlers"
        );

        _activeEventHandlingCounter = _meter.CreateUpDownCounter<long>(
            TelemetryTags.Queries.ActiveQueriesNumber,
            "{query}",
            "Number of queries currently being handled"
        );

        _eventHandlingDuration = _meter.CreateHistogram<double>(
            TelemetryTags.Queries.QueryHandlingDuration,
            "s",
            "Measures the duration of inbound queries"
        );
    }

    public void Dispose()
    {
        _meter.Dispose();
    }

    public long QueryHandlingStart(string queryType)
    {
        var tags = new TagList { { TelemetryTags.Queries.QueryType, queryType } };

        if (_activeEventHandlingCounter.Enabled)
        {
            _activeEventHandlingCounter.Add(1, tags);
        }

        if (_totalQueriesNumber.Enabled)
        {
            _totalQueriesNumber.Add(1, tags);
        }

        return _timeProvider.GetTimestamp();
    }

    public void QueryHandlingEnd(string queryType, long startingTimestamp)
    {
        var tags =
            _activeEventHandlingCounter.Enabled || _eventHandlingDuration.Enabled
                ? new TagList { { TelemetryTags.Queries.QueryType, queryType } }
                : default;

        if (_activeEventHandlingCounter.Enabled)
        {
            _activeEventHandlingCounter.Add(-1, tags);
        }

        if (!_eventHandlingDuration.Enabled)
        {
            return;
        }

        var elapsed = _timeProvider.GetElapsedTime(startingTimestamp);

        _eventHandlingDuration.Record(elapsed.TotalSeconds, tags);
    }
}
