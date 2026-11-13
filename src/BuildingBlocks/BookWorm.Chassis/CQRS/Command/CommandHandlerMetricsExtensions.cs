using System.Diagnostics.Metrics;
using BookWorm.Chassis.OpenTelemetry;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace BookWorm.Chassis.CQRS.Command;

public static partial class CommandHandlerMetricsExtensions
{
    [Counter<long>(
        TelemetryTags.Commands.CommandType,
        Name = TelemetryTags.Commands.TotalCommandsNumber,
        Unit = "{command}",
        Description = "Total number of commands send to command handlers"
    )]
    public static partial TotalCommandsNumber CreateTotalCommandsNumber(Meter meter);

    [Histogram<double>(
        TelemetryTags.Commands.CommandType,
        Name = TelemetryTags.Commands.CommandHandlingDuration,
        Unit = "s",
        Description = "Measures the duration of inbound commands"
    )]
    public static partial CommandHandlingDuration CreateCommandHandlingDuration(Meter meter);
}
