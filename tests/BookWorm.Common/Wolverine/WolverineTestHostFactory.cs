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
    ///     Creates and starts an in-memory Wolverine host suitable for handler unit tests.
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
