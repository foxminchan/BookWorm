# 6. Runtime View

## 6.1 Key Scenarios

### Scenario 1: Book Search and Browse

```mermaid
sequenceDiagram
    participant U as User
    participant G as API Gateway
    participant C as Catalog Service
    participant V as Vector DB (Qdrant)
    participant R as Redis Cache
    
    U->>G: GET /api/books/search?q="fiction"
    G->>C: Forward search request
    C->>R: Check cache for search results
    alt Cache miss
        C->>V: Vector similarity search
        V-->>C: Relevant book vectors
        C->>C: Combine with text search
        C->>R: Cache search results
    end
    C-->>G: Search results
    G-->>U: JSON response with books
```

### Scenario 2: Order Processing Workflow

```mermaid
sequenceDiagram
    participant U as User
    participant G as API Gateway
    participant B as Basket Service
    participant O as Ordering Service
    participant F as Finance Service
    participant N as Notification Service
    participant E as Event Bus
    
    U->>G: POST /api/orders (create order)
    G->>O: Forward order creation
    O->>B: Get basket contents
    B-->>O: Basket items
    O->>O: Create order aggregate
    O->>E: Publish OrderCreated event
    
    E->>F: OrderCreated event
    F->>F: Process payment
    F->>E: Publish PaymentProcessed event
    
    E->>O: PaymentProcessed event
    O->>O: Update order status
    O->>E: Publish OrderConfirmed event
    
    E->>N: OrderConfirmed event
    N->>N: Send confirmation email
    
    O-->>G: Order confirmation
    G-->>U: Order success response
```

### Scenario 3: Chat Service Interaction

```mermaid
sequenceDiagram
    participant U as User
    participant G as API Gateway
    participant Ch as Chat Service
    participant AI as AI Service (Ollama)
    participant C as Catalog Service
    
    U->>G: POST /api/chat/message
    G->>Ch: Forward chat message
    Ch->>AI: Process natural language
    AI-->>Ch: Intent and entities
    
    alt Book recommendation request
        Ch->>C: Get book recommendations
        C-->>Ch: Recommended books
    end
    
    Ch->>AI: Generate response with context
    AI-->>Ch: AI-generated response
    Ch-->>G: Chat response
    G-->>U: JSON response with answer
```

## 6.2 System Startup Sequence

```mermaid
sequenceDiagram
    participant A as Aspire Host
    participant I as Infrastructure
    participant S as Services
    participant G as Gateway
    
    A->>I: Start infrastructure (DB, Redis, RabbitMQ)
    I-->>A: Infrastructure ready
    
    A->>S: Start microservices
    S->>I: Connect to databases
    S->>I: Connect to message bus
    S-->>A: Services ready
    
    A->>G: Start API Gateway
    G->>S: Register service endpoints
    G-->>A: Gateway ready
    
    A->>A: System startup complete
```

## 6.3 Event-Driven Flows

### Order Saga Pattern
```mermaid
stateDiagram-v2
    [*] --> OrderInitiated
    OrderInitiated --> PaymentPending : OrderCreated
    PaymentPending --> PaymentCompleted : PaymentProcessed
    PaymentPending --> PaymentFailed : PaymentRejected
    PaymentCompleted --> OrderConfirmed : InventoryReserved
    PaymentCompleted --> CompensationStarted : InventoryUnavailable
    PaymentFailed --> OrderCancelled : CancelOrder
    CompensationStarted --> OrderCancelled : RefundProcessed
    OrderConfirmed --> [*]
    OrderCancelled --> [*]
```

### Event Flow Patterns
| Event Type | Publisher | Subscribers | Purpose |
|------------|-----------|-------------|---------|
| **BookCreated** | Catalog Service | Search Index, Analytics | Catalog updates |
| **OrderCreated** | Ordering Service | Finance, Notification | Order processing |
| **PaymentProcessed** | Finance Service | Ordering, Notification | Payment confirmation |
| **OrderConfirmed** | Ordering Service | Inventory, Notification | Order fulfillment |
| **UserRegistered** | Identity Service | Notification, Analytics | User onboarding |

## 6.4 Error Handling Flows

### Circuit Breaker Pattern
```mermaid
sequenceDiagram
    participant S as Service A
    participant CB as Circuit Breaker
    participant T as Service B
    
    S->>CB: Request to Service B
    CB->>T: Forward request
    T-->>CB: Timeout/Error
    CB->>CB: Increment failure count
    
    alt Failure threshold reached
        CB->>CB: Open circuit
        S->>CB: Next request
        CB-->>S: Fail fast (cached response)
    end
    
    Note over CB: After timeout period
    CB->>CB: Half-open circuit
    S->>CB: Test request
    CB->>T: Forward test request
    T-->>CB: Success
    CB->>CB: Close circuit
```

### Retry Mechanisms
- **Exponential Backoff**: For transient failures
- **Dead Letter Queues**: For permanently failed messages
- **Compensation Actions**: For distributed transaction rollbacks

## 6.5 Performance Optimization Flows

### Caching Strategy
```mermaid
graph TD
    A[Client Request] --> B{Cache Check}
    B -->|Hit| C[Return Cached Data]
    B -->|Miss| D[Query Database]
    D --> E[Store in Cache]
    E --> F[Return Data]
    
    G[Background Job] --> H[Cache Warming]
    H --> I[Preload Popular Data]
```

### Load Balancing
- **Gateway Level**: YARP load balancing across service instances
- **Database Level**: Read replicas for query optimization
- **Cache Level**: Distributed caching with Redis Cluster

## 6.6 Monitoring and Observability

### Telemetry Flow
```mermaid
graph LR
    A[Services] --> B[OpenTelemetry]
    B --> C[Traces]
    B --> D[Metrics]
    B --> E[Logs]
    
    C --> F[Jaeger/Zipkin]
    D --> G[Prometheus]
    E --> H[Elasticsearch]
    
    F --> I[APM Dashboard]
    G --> I
    H --> I
```

### Health Check Flow
- **Startup Checks**: Verify external dependencies
- **Readiness Checks**: Service ready to accept traffic
- **Liveness Checks**: Service is functioning correctly
- **Dependency Checks**: External service availability