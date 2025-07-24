using BookWorm.Basket;
using BookWorm.Catalog;
using BookWorm.Chassis;
using BookWorm.Chat;
using BookWorm.Constants;
using BookWorm.Finance;
using BookWorm.Notification;
using BookWorm.Ordering;
using BookWorm.Rating;
using BookWorm.SharedKernel;
using Assembly = System.Reflection.Assembly;

namespace BookWorm.ArchTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly BasketAssembly = typeof(IBasketApiMarker).Assembly;
    protected static readonly Assembly CatalogAssembly = typeof(ICatalogApiMarker).Assembly;
    protected static readonly Assembly ChatAssembly = typeof(IChatApiMarker).Assembly;
    protected static readonly Assembly FinanceAssembly = typeof(IFinanceApiMarker).Assembly;

    protected static readonly Assembly NotificationAssembly =
        typeof(INotificationApiMarker).Assembly;

    protected static readonly Assembly OrderingAssembly = typeof(IOrderingApiMarker).Assembly;
    protected static readonly Assembly RatingAssembly = typeof(IRatingApiMarker).Assembly;
    protected static readonly Assembly ChassisAssembly = typeof(IChassisMarker).Assembly;
    protected static readonly Assembly ConstantsAssembly = typeof(IConstantsMarker).Assembly;
    protected static readonly Assembly SharedKernelAssembly = typeof(ISharedKernelMarker).Assembly;
}
