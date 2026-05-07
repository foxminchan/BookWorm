using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace BookWorm.Common.Wolverine;

/// <summary>
///     Factory that creates a minimal in-memory WolverineFx <see cref="IHost" /> for unit tests.
///     No Kafka transport and no PostgreSQL persistence — messages are dispatched in-process only.
/// </summary>
public static class WolverineTestHostFactory
{
    /// <summary>
    ///     Registers a catch-all <see cref="ActivityListener" /> that enables sampling for all
    ///     <see cref="ActivitySource" /> instances (including Wolverine's own source) so that
    ///     <see cref="Activity" /> objects are created and populated during test execution.
    ///     Call this once per test run / test class setup.
    /// </summary>
    /// <returns>The registered listener; dispose it when the test is torn down.</returns>
    public static ActivityListener AddOtelListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    /// <summary>
    ///     Creates and starts an in-memory Wolverine host suitable for handler unit tests.
    ///     An <see cref="ActivityListener" /> is registered automatically so that Wolverine's
    ///     built-in OpenTelemetry instrumentation produces real (non-null) <see cref="Activity" />
    ///     spans during the test.
    /// </summary>
    /// <param name="configure">Optional additional <see cref="WolverineOptions" /> customization.</param>
    /// <param name="includeAssemblies">
    ///     Assemblies to scan for message handlers. Pass the assembly that contains the handler
    ///     under test (e.g. <c>typeof(MyHandler).Assembly</c>).
    /// </param>
    /// <returns>
    ///     The started <see cref="IHost" /> and the resolved <see cref="IMessageBus" />.
    ///     Dispose the host after the test to release resources.
    /// </returns>
    public static async Task<(IHost Host, IMessageBus Bus)> CreateAsync(
        Action<WolverineOptions>? configure = null,
        params System.Reflection.Assembly[] includeAssemblies
    )
    {
        // Register the dummy listener before the host starts so Wolverine's ActivitySource
        // is sampled from the very first message dispatch.
        AddOtelListener();

        var host = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                foreach (var assembly in includeAssemblies)
                {
                    opts.Discovery.IncludeAssembly(assembly);
                }

                configure?.Invoke(opts);
            })
            .StartAsync();

        var bus = host.Services.GetRequiredService<IMessageBus>();
        return (host, bus);
    }
}
