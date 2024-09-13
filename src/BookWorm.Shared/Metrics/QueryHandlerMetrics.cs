using System.Diagnostics.Metrics;

namespace BookWorm.Shared.Metrics;

public sealed class QueryHandlerMetrics : IDisposable
{
    private readonly UpDownCounter<long> _activeEventHandlingCounter;
    private readonly Histogram<double> _eventHandlingDuration;
    private readonly Meter _meter;
    private readonly TimeProvider _timeProvider;
    private readonly Counter<long> _totalCommandsNumber;

    public QueryHandlerMetrics(IMeterFactory meterFactory, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        _totalCommandsNumber = _meter.CreateCounter<long>(
            TelemetryTags.Queries.TotalQueriesNumber,
            "{query}",
            "Total number of queries send to query handlers");

        _activeEventHandlingCounter = _meter.CreateUpDownCounter<long>(
            TelemetryTags.Queries.ActiveQueriesNumber,
            "{query}",
            "Number of queries currently being handled");

        _eventHandlingDuration = _meter.CreateHistogram<double>(
            TelemetryTags.Queries.QueryHandlingDuration,
            "s",
            "Measures the duration of inbound queries");
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

        if (_totalCommandsNumber.Enabled)
        {
            _totalCommandsNumber.Add(1, tags);
        }

        return _timeProvider.GetTimestamp();
    }

    public void QueryHandlingEnd(string queryType, long startingTimestamp)
    {
        var tags = _activeEventHandlingCounter.Enabled
                   || _eventHandlingDuration.Enabled
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

        _eventHandlingDuration.Record(
            elapsed.TotalSeconds,
            tags);
    }
}
