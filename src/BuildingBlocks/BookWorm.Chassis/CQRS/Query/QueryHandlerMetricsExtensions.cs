using System.Diagnostics.Metrics;
using BookWorm.Chassis.OpenTelemetry;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace BookWorm.Chassis.CQRS.Query;

public static partial class QueryHandlerMetricsExtensions
{
    [Counter<long>(
        TelemetryTags.Queries.QueryType,
        Name = TelemetryTags.Queries.TotalQueriesNumber,
        Unit = "{query}",
        Description = "Total number of queries send to query handlers"
    )]
    public static partial TotalQueriesNumber CreateTotalQueriesNumber(Meter meter);

    [Histogram<double>(
        TelemetryTags.Queries.QueryType,
        Name = TelemetryTags.Queries.QueryHandlingDuration,
        Unit = "s",
        Description = "Measures the duration of inbound queries"
    )]
    public static partial QueryHandlingDuration CreateQueryHandlingDuration(Meter meter);
}
