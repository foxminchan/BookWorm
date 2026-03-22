using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.OpenTelemetry;
using Mediator;

namespace BookWorm.Ordering.Extensions;

internal static class CqrsExtensions
{
    public static void AddCqrsInfrastructure(this IServiceCollection services)
    {
        // Configure Mediator
        services
            .AddMediator(
                (MediatorOptions options) => options.ServiceLifetime = ServiceLifetime.Scoped
            )
            .ApplyLoggingBehavior()
            .ApplyActivityBehavior()
            .ApplyValidationBehavior()
            .ApplyTransactionBehavior<OrderingDbContext>();

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IOrderingApiMarker>(
            includeInternalTypes: true
        );

        services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

        services.AddEventDispatcher();
        services.AddScoped<IEventMapper, EventMapper>();
        services.AddScoped<IRequestManager, RequestManager>();
    }
}
