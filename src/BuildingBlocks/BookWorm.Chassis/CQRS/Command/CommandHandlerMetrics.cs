using System.Diagnostics;
using System.Diagnostics.Metrics;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;

namespace BookWorm.Chassis.CQRS.Command;

internal sealed class CommandHandlerMetrics : IDisposable
{
    private readonly UpDownCounter<long> _activeEventHandlingCounter;
    private readonly CommandHandlingDuration _eventHandlingDuration;
    private readonly Meter _meter;
    private readonly TimeProvider _timeProvider;
    private readonly TotalCommandsNumber _totalCommandsNumber;

    public CommandHandlerMetrics(IMeterFactory meterFactory, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        _totalCommandsNumber = CommandHandlerMetricsExtensions.CreateTotalCommandsNumber(_meter);

        _activeEventHandlingCounter = _meter.CreateUpDownCounter<long>(
            TelemetryTags.Commands.ActiveCommandsNumber,
            "{command}",
            "Number of commands currently being handled"
        );

        _eventHandlingDuration = CommandHandlerMetricsExtensions.CreateCommandHandlingDuration(
            _meter
        );
    }

    public void Dispose()
    {
        _meter.Dispose();
    }

    public long CommandHandlingStart(string commandType)
    {
        if (_activeEventHandlingCounter.Enabled)
        {
            var tags = new TagList { { TelemetryTags.Commands.CommandType, commandType } };
            _activeEventHandlingCounter.Add(1, tags);
        }

        _totalCommandsNumber.Add(1, commandType);

        return _timeProvider.GetTimestamp();
    }

    public void CommandHandlingEnd(string commandType, long startingTimestamp)
    {
        if (_activeEventHandlingCounter.Enabled)
        {
            var tags = new TagList { { TelemetryTags.Commands.CommandType, commandType } };
            _activeEventHandlingCounter.Add(-1, tags);
        }

        var elapsed = _timeProvider.GetElapsedTime(startingTimestamp);

        _eventHandlingDuration.Record(elapsed.TotalSeconds, commandType);
    }
}
