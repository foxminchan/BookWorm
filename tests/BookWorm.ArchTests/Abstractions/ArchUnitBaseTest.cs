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
            SchedulerAssembly,
            ChassisAssembly,
            ConstantsAssembly,
            SharedKernelAssembly
        )
        .Build();

    protected static readonly IObjectProvider<IType> BasketServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(BasketAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Basket));

    protected static readonly IObjectProvider<IType> CatalogServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(CatalogAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Catalog));

    protected static readonly IObjectProvider<IType> ChatServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(ChatAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Chat));

    protected static readonly IObjectProvider<IType> FinanceServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(FinanceAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Finance));

    protected static readonly IObjectProvider<IType> NotificationServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(NotificationAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Notification));

    protected static readonly IObjectProvider<IType> OrderingServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(OrderingAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Ordering));

    protected static readonly IObjectProvider<IType> RatingServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(RatingAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Rating));

    protected static readonly IObjectProvider<IType> SchedulerServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(SchedulerAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Scheduler));

    protected static readonly IObjectProvider<IType> ChassisServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(ChassisAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Chassis));

    protected static readonly IObjectProvider<IType> ConstantsServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(ConstantsAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(Constants));

    protected static readonly IObjectProvider<IType> SharedKernelServiceTypes = ArchRuleDefinition
        .Types()
        .That()
        .ResideInAssembly(SharedKernelAssembly)
        .And()
        .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
        .As(nameof(SharedKernel));
}
