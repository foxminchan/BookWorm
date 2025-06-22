---
category:
  - Architecture Decisions Records
tag:
  - ADR
---

# ADR-002: Event-Driven Architecture with CQRS

## Status

**Accepted** - July 2024

## Context

The microservices architecture requires loose coupling between services while maintaining data consistency and providing audit capabilities. Different services have varying read and write performance requirements:

- **High Write Volume**: Order processing, rating submissions
- **High Read Volume**: Book catalog browsing, search operations
- **Audit Requirements**: Financial transactions, order history
- **Data Consistency**: Cross-service business operations
- **Service Integration**: Services need to react to business events

Traditional request-response patterns would create tight coupling and don't provide the scalability and auditability required.

## Decision

Implement event-driven architecture with Command Query Responsibility Segregation (CQRS) pattern.

### Architecture Components

```cs
// Command side - write operations
public sealed record CreateAuthorCommand(string Name) : ICommand<Guid>;

public sealed class CreateAuthorHandler(IAuthorRepository repository)
    : ICommandHandler<CreateAuthorCommand, Guid>
{
    public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.AddAsync(new(request.Name), cancellationToken);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}

// Event - represents what happened
public sealed class BookCreatedEvent(Book book) : DomainEvent
{
    public Book Book { get; init; } = book;
}

// Query side - read operations
public sealed record ListCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;

public sealed class ListCategoriesHandler(ICategoryRepository repository)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(
        ListCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var categories = await repository.ListAsync(cancellationToken);

        return categories.ToCategoryDtos();
    }
}
```

### Event Flow

1. **Command Processing**: Commands modify aggregate state
2. **Event Generation**: Aggregates generate domain events
3. **Event Publishing**: Events are published to message bus
4. **Event Handling**: Other services react to relevant events
5. **Read Model Updates**: Query models are updated asynchronously

## Rationale

### Why Event-Driven + CQRS?

1. **Scalability**: Separate read and write workloads for independent optimization
2. **Consistency**: Eventual consistency through events is acceptable for most use cases
3. **Auditability**: Event sourcing provides complete audit trail of all changes
4. **Service Integration**: Loose coupling through event-driven communication
5. **Performance**: Optimized read models for query scenarios

### Event Store Strategy

- **Database-based**: PostgreSQL with custom event store tables
- **Message Bus**: Redis Streams for event distribution
- **Projections**: Materialized views for optimized queries

## Consequences

### Positive Outcomes

- **Improved Scalability**: Read and write workloads optimized independently
- **Better Separation of Concerns**: Clear distinction between commands and queries
- **Comprehensive Audit Trail**: Complete history of all system changes
- **Loose Coupling**: Services communicate through events, not direct calls
- **Performance Optimization**: Read models tailored for specific query patterns
- **Resilience**: Asynchronous processing provides better fault tolerance

### Negative Outcomes

- **Eventual Consistency Complexity**:
  - Users may see stale data temporarily
  - Complex scenarios require careful handling
- **Development Complexity**:
  - More complex programming model
  - Event versioning and schema evolution
  - Debugging distributed flows
- **Infrastructure Requirements**:
  - Event store and message bus
  - Additional monitoring and tooling
  - Projection rebuild mechanisms

### Mitigation Strategies

- **Event Versioning**: Implement event schema versioning from the start
- **Saga Pattern**: Use sagas for complex multi-service transactions
- **Monitoring**: Comprehensive event flow monitoring and alerting
- **Testing**: Event-driven testing strategies and tools
- **Documentation**: Clear event contracts and flow documentation

## Implementation Details

### Message Bus Configuration

```csharp
public static class Extensions
{
    public static void AddEventBus(
        this IHostApplicationBuilder builder,
        Type type,
        Action<IBusRegistrationConfigurator>? configure = null
    )
    {
        var connectionString = builder.Configuration.GetConnectionString(Components.Queue);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        builder.Services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.AddConsumers(type.Assembly);

            config.AddSagas(type.Assembly);

            config.AddActivities(type.Assembly);

            config.AddRequestClient(type);

            config.UsingRabbitMq(
                (context, configurator) =>
                {
                    configurator.Host(new Uri(connectionString));
                    configurator.ConfigureEndpoints(context);
                    configurator.UseMessageRetry(AddRetryConfiguration);
                }
            );

            configure?.Invoke(config);
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
            .WithTracing(p => p.AddSource(DiagnosticHeaders.DefaultListenerName));
    }

    private static void AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(
                3,
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMinutes(120),
                TimeSpan.FromMilliseconds(200)
            )
            .Ignore<ValidationException>();
    }
}
```

### Event Handler Example

```csharp
public sealed class OrderEventHandler(
    IEventDispatcher eventDispatcher,
    IDocumentSession documentSession,
    ILogger<OrderEventHandler> logger
)
    : INotificationHandler<OrderPlacedEvent>,
        INotificationHandler<OrderCompletedEvent>,
        INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        OrderingTrace.LogOrderCancelled(logger, notification.Order.Id, Status.New);
        await eventDispatcher.DispatchAsync(notification, cancellationToken);
        await documentSession.GetAndUpdate<OrderSummary>(
            Guid.CreateVersion7(),
            notification,
            cancellationToken
        );
    }
}
```

## Event Catalog Integration

Events are documented in the EventCatalog:

- Event schemas and versions
- Producer and consumer mappings
- Event flow documentation
- Change impact analysis

## Related Decisions

- [ADR-001: Microservices Architecture](adr-001-microservices-architecture.md)
- [ADR-004: PostgreSQL as Primary Database](adr-004-postgresql-database.md)
- [ADR-008: API Gateway Pattern Implementation](adr-008-api-gateway.md)

## Future Considerations

- **Event Sourcing**: Consider full event sourcing for critical aggregates
- **Saga Orchestration**: Implement saga pattern for complex workflows
- **Event Mesh**: Evaluate event mesh patterns for complex event routing
