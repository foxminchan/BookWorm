using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookWorm.Finance.ContractTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyWolverine.Initialize();
        RegisterOtelListener();
    }

    private static void RegisterOtelListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
        };

        ActivitySource.AddActivityListener(listener);
    }
}
