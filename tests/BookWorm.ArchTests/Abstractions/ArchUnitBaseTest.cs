using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;

namespace BookWorm.ArchTests.Abstractions;

public abstract class ArchUnitBaseTest : BaseTest
{
    protected static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            BasketAssembly,
            CatalogAssembly,
            ChatAssembly,
            FinanceAssembly,
            NotificationAssembly,
            OrderingAssembly,
            RatingAssembly,
            ChassisAssembly,
            ConstantsAssembly,
            SharedKernelAssembly
        )
        .Build();

    protected static readonly IObjectProvider<IType> BasketServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(BasketAssembly)
        .As(nameof(Basket));

    protected static readonly IObjectProvider<IType> CatalogServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(CatalogAssembly)
        .As(nameof(Catalog));

    protected static readonly IObjectProvider<IType> ChatServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(ChatAssembly)
        .As(nameof(Chat));

    protected static readonly IObjectProvider<IType> FinanceServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(FinanceAssembly)
        .As(nameof(Finance));

    protected static readonly IObjectProvider<IType> NotificationServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(NotificationAssembly)
        .As(nameof(Notification));

    protected static readonly IObjectProvider<IType> OrderingServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(OrderingAssembly)
        .As(nameof(Ordering));

    protected static readonly IObjectProvider<IType> RatingServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(RatingAssembly)
        .As(nameof(Rating));

    protected static readonly IObjectProvider<IType> ChassisServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(ChassisAssembly)
        .As(nameof(Chassis));

    protected static readonly IObjectProvider<IType> ConstantsServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(ConstantsAssembly)
        .As(nameof(Constants));

    protected static readonly IObjectProvider<IType> SharedKernelServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(SharedKernelAssembly)
        .As(nameof(SharedKernel));
}
