using System.Runtime.CompilerServices;

namespace BookWorm.Basket.ContractTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyMassTransit.Initialize();
    }
}
