using System.Runtime.CompilerServices;

namespace BookWorm.Catalog.IntegrationTests;

/// <summary>
/// Disables configuration file watchers to prevent file descriptor exhaustion
/// in Linux CI environments where integration tests start IHost instances.
/// </summary>
internal static class TestEnvironmentInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE", "false");
    }
}
