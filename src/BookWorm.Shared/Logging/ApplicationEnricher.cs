namespace BookWorm.Shared.Logging;

/// <summary>
///    ref: https://andrewlock.net/customising-the-new-telemetry-logging-source-generator/
/// </summary>
public sealed class ApplicationEnricher : IStaticLogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add("MachineName", Environment.MachineName);
    }
}
