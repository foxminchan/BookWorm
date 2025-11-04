using System.Runtime.CompilerServices;

namespace BookWorm.Catalog.ContractTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyMassTransit.Initialize();
    }
}
