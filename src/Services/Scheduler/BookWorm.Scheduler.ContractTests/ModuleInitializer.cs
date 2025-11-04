using System.Runtime.CompilerServices;

namespace BookWorm.Scheduler.ContractTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyMassTransit.Initialize();
    }
}
