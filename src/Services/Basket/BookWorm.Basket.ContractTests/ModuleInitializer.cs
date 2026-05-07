using System.Runtime.CompilerServices;
using VerifyTests;

namespace BookWorm.Basket.ContractTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyWolverine.Initialize();
    }
}
