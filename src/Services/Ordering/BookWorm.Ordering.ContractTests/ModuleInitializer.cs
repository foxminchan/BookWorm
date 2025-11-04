using System.Runtime.CompilerServices;

namespace BookWorm.Ordering.ContractTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyMassTransit.Initialize();
    }
}
