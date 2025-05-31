# 6. Runtime View

## 6.1 Important Scenarios

This section describes the dynamic behavior of BookWorm through key runtime scenarios, showing how the building blocks interact to fulfill important use cases.

## 6.2 Book Purchase Scenario

### Complete Order Processing Flow

```mermaid
sequenceDiagram
    participant Customer
    participant WebApp
    participant Gateway
    participant Basket
    participant Ordering
    participant Finance
    participant Catalog
    participant EventBus
    participant Notification
    
    Customer->>WebApp: Add book to cart
    WebApp->>Gateway: POST /basket/items
    Gateway->>Basket: Add item to basket
    Basket-->>Gateway: Item added
    Gateway-->>WebApp: Success response
    
    Customer->>WebApp: Proceed to checkout
    WebApp->>Gateway: POST /orders/checkout
    Gateway->>Ordering: Create order from basket
    
    Ordering->>Basket: Get basket items
    Basket-->>Ordering: Basket contents
    
    Ordering->>Catalog: Verify book availability
    Catalog-->>Ordering: Availability confirmed
    
    Ordering->>EventBus: Publish OrderCreated event
    EventBus->>Finance: Process financial transaction
    Finance-->>EventBus: Transaction processed
    
    Ordering->>EventBus: Publish OrderConfirmed event
    EventBus->>Notification: Send confirmation email
    EventBus->>Basket: Clear customer basket
    
    Ordering-->>Gateway: Order confirmed
    Gateway-->>WebApp: Order success
    WebApp-->>Customer: Order confirmation
```

### Key Runtime Aspects

1. **Basket to Order Transition**: Atomic conversion ensuring data consistency
2. **Book Availability**: Verification of catalog items during order processing
3. **Financial Processing**: Integration with finance service for transactions
4. **Event-Driven Notifications**: Asynchronous email and cleanup processes
5. **Error Handling**: Saga pattern for rollback on failures

## 6.3 Book Search and Discovery

### AI-Enhanced Search Flow

```mermaid
sequenceDiagram
    participant User
    participant WebApp
    participant Gateway
    participant Catalog
    participant AIService
    participant SearchIndex
    participant Cache
    
    User->>WebApp: Enter search query
    WebApp->>Gateway: GET /search?q=query
    Gateway->>Cache: Check cached results
    
    alt Cache Hit
        Cache-->>Gateway: Return cached results
        Gateway-->>WebApp: Search results
    else Cache Miss
        Gateway->>Catalog: Process search request
        
        Catalog->>AIService: Generate text embedding
        AIService-->>Catalog: Vector embedding
        
        Catalog->>SearchIndex: Vector similarity search
        SearchIndex-->>Catalog: Similar books
        
        Catalog->>SearchIndex: Text search
        SearchIndex-->>Catalog: Text matches
        
        Catalog->>Catalog: Merge and rank results
        Catalog-->>Gateway: Ranked search results
        
        Gateway->>Cache: Cache results
        Gateway-->>WebApp: Search results
    end
    
    WebApp-->>User: Display search results
```

### Search Performance Optimizations

- **Multi-level Caching**: Redis cache for frequently searched terms
- **Vector Search**: AI embeddings for semantic similarity
- **Hybrid Search**: Combination of text and vector search
- **Real-time Indexing**: Immediate search index updates for new books

## 6.4 Real-time Chat Scenario

### Customer Support Chat Flow

```mermaid
sequenceDiagram
    participant Customer
    participant WebApp
    participant ChatHub
    participant ChatService
    participant AIBot
    participant SupportAgent
    participant Database
    
    Customer->>WebApp: Initiate chat
    WebApp->>ChatHub: Connect to chat
    ChatHub->>ChatService: Create conversation
    ChatService->>Database: Store conversation
    
    Customer->>ChatHub: Send message
    ChatHub->>ChatService: Process message
    ChatService->>Database: Store message
    
    ChatService->>AIBot: Analyze message intent
    AIBot-->>ChatService: Intent classification
    
    alt Simple Query
        ChatService->>AIBot: Generate response
        AIBot-->>ChatService: AI response
        ChatService->>ChatHub: Send AI response
        ChatHub-->>Customer: AI answer
    else Complex Issue
        ChatService->>SupportAgent: Route to human agent
        SupportAgent->>ChatHub: Join conversation
        ChatHub-->>Customer: Agent joined
        
        Customer->>ChatHub: Explain issue
        ChatHub->>SupportAgent: Forward message
        SupportAgent->>ChatHub: Provide solution
        ChatHub-->>Customer: Agent response
    end
    
    ChatService->>Database: Update conversation status
```

### Chat System Features

- **Real-time Messaging**: SignalR for instant message delivery
- **AI Triage**: Automatic classification of customer inquiries
- **Agent Routing**: Intelligent assignment to available support agents
- **Message Persistence**: MongoDB for chat history storage
- **Presence Indicators**: Real-time user status updates

## 6.5 Event-Driven Architecture Flow

### Cross-Service Event Processing

```mermaid
sequenceDiagram
    participant OrderingService
    participant EventBus
    participant CatalogService
    participant RatingService
    participant EmailService
    
    OrderingService->>EventBus: Publish OrderCompleted event
    
    par Inventory Update
        EventBus->>CatalogService: OrderCompleted event
        CatalogService->>CatalogService: Update inventory
        CatalogService->>EventBus: InventoryUpdated event
    and Rating Invitation
        EventBus->>RatingService: OrderCompleted event
        RatingService->>EmailService: Send rating invitation
    end
    
    Note over EventBus: Events processed independently
    Note over EventBus: Eventual consistency maintained
```

### Event Processing Patterns

| Pattern | Implementation | Purpose |
|---------|----------------|---------|
| **Outbox Pattern** | Database transaction + event publishing | Ensures reliable event publishing |
| **Inbox Pattern** | Idempotent event processing | Prevents duplicate processing |
| **Event Sourcing** | Events as source of truth | Provides audit trail and temporal queries |
| **Saga Orchestration** | Centralized workflow coordination | Manages complex business processes |
| **Saga Choreography** | Decentralized event reactions | Enables loose coupling between services |

## 6.6 System Startup and Health Monitoring

### Service Discovery and Health Checks

```mermaid
sequenceDiagram
    participant ServiceRegistry
    participant Gateway
    participant CatalogService
    participant OrderingService
    participant HealthMonitor
    
    CatalogService->>ServiceRegistry: Register service
    OrderingService->>ServiceRegistry: Register service
    
    Gateway->>ServiceRegistry: Discover services
    ServiceRegistry-->>Gateway: Service endpoints
    
    loop Health Monitoring
        HealthMonitor->>CatalogService: Health check
        CatalogService-->>HealthMonitor: Health status
        
        HealthMonitor->>OrderingService: Health check
        OrderingService-->>HealthMonitor: Health status
    end
    
    Note over HealthMonitor: Unhealthy services removed from routing
```

### Observability and Monitoring

- **Distributed Tracing**: OpenTelemetry for request correlation across services
- **Structured Logging**: Serilog with correlation IDs and contextual information
- **Metrics Collection**: Prometheus-compatible metrics for performance monitoring
- **Health Dashboards**: Real-time system health visualization
- **Alerting**: Automated alerts for critical system issues

## 6.7 Error Handling and Recovery

### Circuit Breaker Pattern Implementation

```mermaid
stateDiagram-v2
    [*] --> Closed
    Closed --> Open: Failure threshold reached
    Open --> HalfOpen: Timeout expires
    HalfOpen --> Closed: Success
    HalfOpen --> Open: Failure
    
    note right of Closed
        Normal operation
        Requests flow through
    end note
    
    note right of Open
        Circuit breaker trips
        Fast fail responses
    end note
    
    note right of HalfOpen
        Limited test requests
        Evaluate service health
    end note
```

### Resilience Strategies

| Strategy | Implementation | Use Case |
|----------|----------------|----------|
| **Retry Policies** | Exponential backoff with jitter | Transient failures |
| **Circuit Breakers** | Polly library integration | Cascading failure prevention |
| **Timeouts** | Configurable per operation | Resource protection |
| **Bulkhead Isolation** | Separate thread pools | Fault isolation |
| **Graceful Degradation** | Fallback responses | Service unavailability |