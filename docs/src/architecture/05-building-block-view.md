# 5. Building Block View

## 5.1 Whitebox Overall System

### System Overview

BookWorm is decomposed into distinct microservices following Domain-Driven Design principles, with each service owning its data and business logic.

```mermaid
graph TB
    subgraph "Client Layer"
        WebApp[🖥️ Web Application]
        Mobile[📱 Mobile App]
        Admin[⚙️ Admin Dashboard]
    end
    
    subgraph "API Gateway Layer"
        Gateway[🌐 API Gateway<br/>Routing, Auth, Rate Limiting]
    end
    
    subgraph "Application Services"
        CatalogAPI[📚 Catalog API<br/>Books, Categories]
        BasketAPI[🛍️ Basket API<br/>Shopping Cart]
        OrderingAPI[📋 Ordering API<br/>Orders, Checkout]
        FinanceAPI[💰 Finance API<br/>Financial Transactions]
        RatingAPI[⭐ Rating API<br/>Reviews, Ratings]
        ChatAPI[💬 Chat API<br/>Customer Support]
        NotificationAPI[📢 Notification API<br/>Email, Push Notifications]
    end
    
    subgraph "Infrastructure Services"
        EventBus[📡 Event Bus<br/>Message Broker]
        ConfigService[⚙️ Configuration<br/>Settings Management]
        HealthCheck[🏥 Health Monitoring<br/>Service Health]
    end
    
    subgraph "Data Layer"
        CatalogDB[(📚 Catalog DB<br/>PostgreSQL)]
        BasketDB[(🛍️ Basket DB<br/>PostgreSQL)]
        OrderingDB[(📋 Ordering DB<br/>PostgreSQL)]
        FinanceDB[(💰 Finance DB<br/>PostgreSQL)]
        RatingDB[(⭐ Rating DB<br/>PostgreSQL)]
        ChatDB[(💬 Chat DB<br/>PostgreSQL)]
        NotificationDB[(📢 Notification DB<br/>PostgreSQL)]
        EventStore[(📡 Event Store<br/>PostgreSQL)]
        CacheLayer[(🚀 Redis Cache<br/>Performance)]
    end
    
    subgraph "External Services"
        EmailService[📧 Email Service<br/>SMTP Provider]
    end
    
    WebApp --> Gateway
    Mobile --> Gateway
    Admin --> Gateway
    
    Gateway --> CatalogAPI
    Gateway --> BasketAPI
    Gateway --> OrderingAPI
    Gateway --> FinanceAPI
    Gateway --> RatingAPI
    Gateway --> ChatAPI
    Gateway --> NotificationAPI
    
    CatalogAPI --> CatalogDB
    CatalogAPI --> EventBus
    CatalogAPI --> CacheLayer
    
    BasketAPI --> BasketDB
    BasketAPI --> EventBus
    BasketAPI --> CacheLayer
    
    OrderingAPI --> OrderingDB
    OrderingAPI --> EventBus
    
    FinanceAPI --> FinanceDB
    FinanceAPI --> EventBus
    
    RatingAPI --> RatingDB
    RatingAPI --> EventBus
    
    ChatAPI --> ChatDB
    ChatAPI --> EventBus
    
    NotificationAPI --> NotificationDB
    NotificationAPI --> EmailService
    NotificationAPI --> EventBus
    
    EventBus --> EventStore
```

### Contained Building Blocks

| Component | Responsibility | Technology |
|-----------|----------------|------------|
| **API Gateway** | Request routing, authentication, rate limiting | ASP.NET Core, YARP |
| **Catalog API** | Book catalog management, search functionality | ASP.NET Core, PostgreSQL |
| **Ordering API** | Order processing, payment coordination | ASP.NET Core, PostgreSQL |
| **Basket API** | Shopping cart management | ASP.NET Core, Redis |
| **Rating API** | Reviews and ratings management | ASP.NET Core, PostgreSQL |
| **Chat API** | Real-time communication | ASP.NET Core, SignalR, PostgreSQL |
| **Event Bus** | Asynchronous messaging | RabbitMQ/Azure Service Bus |

## 5.2 Level 2 - Catalog Service

### Catalog Service Whitebox

```mermaid
graph TB
    subgraph "Catalog API"
        Controller[📋 Controllers<br/>REST Endpoints]
        
        subgraph "Application Layer"
            Commands[📝 Commands<br/>Create, Update, Delete]
            Queries[🔍 Queries<br/>Search, Filter, Browse]
            Handlers[⚙️ Command/Query Handlers]
        end
        
        subgraph "Domain Layer"
            Entities[📚 Domain Entities<br/>Book, Author, Category]
            Services[🔧 Domain Services<br/>Search, Recommendations]
            Events[📡 Domain Events<br/>BookCreated, PriceChanged]
        end
        
        subgraph "Infrastructure Layer"
            Repository[🗄️ Repository<br/>Data Access]
            EventPublisher[📡 Event Publisher<br/>Outbox Pattern]
            SearchIndex[🔍 Search Index<br/>Elasticsearch]
        end
    end
    
    Controller --> Commands
    Controller --> Queries
    Commands --> Handlers
    Queries --> Handlers
    Handlers --> Entities
    Handlers --> Services
    Services --> Events
    Handlers --> Repository
    Events --> EventPublisher
    Queries --> SearchIndex
```

### Catalog Service Components

| Component | Purpose | Implementation |
|-----------|---------|----------------|
| **Book Management** | CRUD operations for books | Entity Framework Core |
| **Author Management** | Author profiles and relationships | PostgreSQL with EF Core |
| **Category Management** | Book categorization and hierarchy | Tree structure in database |
| **Search Service** | Full-text search and filtering | Azure Cognitive Search |
| **Price Management** | Dynamic pricing and promotions | Domain services |

## 5.3 Level 2 - Ordering Service

### Ordering Service Whitebox

```mermaid
graph TB
    subgraph "Ordering API"
        OrderController[📦 Order Controller]
        
        subgraph "Application Layer"
            OrderCommands[📝 Order Commands<br/>Create, Update, Cancel]
            OrderQueries[🔍 Order Queries<br/>History, Status]
            SagaOrchestrator[🎭 Saga Orchestrator<br/>Order Processing Workflow]
        end
        
        subgraph "Domain Layer"
            Order[📦 Order Aggregate<br/>Order, OrderItem]
            OrderPolicy[📋 Order Policies<br/>Validation, Business Rules]
        end
        
        subgraph "Infrastructure Layer"
            OrderRepo[🗄️ Order Repository]
            EventHandlers[📡 Event Handlers<br/>Inbox Pattern]
        end
    end
    
    OrderController --> OrderCommands
    OrderController --> OrderQueries
    
    OrderCommands --> SagaOrchestrator
    SagaOrchestrator --> Order
    Order --> OrderPolicy
    
    Order --> OrderRepo
    SagaOrchestrator --> EventHandlers
```

### Ordering Service Components

| Component | Purpose | Implementation |
|-----------|---------|----------------|
| **Order Aggregate** | Order lifecycle management | DDD aggregate pattern |
| **Order Saga** | Complex order workflow | Saga orchestration pattern |
| **Order Events** | Domain event publishing | Event-driven coordination |

## 5.4 Level 2 - Chat Service

### Chat Service Whitebox

```mermaid
graph TB
    subgraph "Chat API"
        ChatHub[💬 SignalR Hub<br/>Real-time Communication]
        ChatController[📱 Chat Controller<br/>REST API]
        
        subgraph "Application Layer"
            MessageCommands[📝 Message Commands<br/>Send, Edit, Delete]
            AICommands[🤖 AI Commands<br/>Bot Responses]
            ChatQueries[🔍 Chat Queries<br/>History, Participants]
        end
        
        subgraph "Domain Layer"
            Conversation[💬 Conversation Aggregate]
            Message[📨 Message Entity]
            Participant[👤 Participant Entity]
            AIBot[🤖 AI Bot Service]
        end
        
        subgraph "Infrastructure Layer"
            ChatRepo[🗄️ Chat Repository<br/>PostgreSQL]
            AIIntegration[🤖 AI Integration<br/>Gemma 3 API]
            NotificationService[📧 Notification Service<br/>Email/Push]
        end
    end
    
    ChatHub --> MessageCommands
    ChatController --> ChatQueries
    MessageCommands --> Conversation
    AICommands --> AIBot
    Conversation --> Message
    Conversation --> Participant
    AIBot --> AIIntegration
    Conversation --> ChatRepo
    Message --> NotificationService
```

### Chat Service Components

| Component | Purpose | Implementation |
|-----------|---------|----------------|
| **Real-time Messaging** | Live chat functionality | SignalR for WebSockets |
| **AI Chatbot** | Automated customer support | Gemma 3 integration |
| **Message Persistence** | Chat history storage | PostgreSQL database |
| **Notification System** | Alert delivery | Email and push notifications |
| **Presence Management** | User online status | In-memory state with Redis |

## 5.5 Cross-Cutting Concerns

### Shared Infrastructure Components

| Component | Purpose | Used By |
|-----------|---------|---------|
| **Authentication** | JWT token validation | All API services |
| **Logging** | Structured logging | All services |
| **Monitoring** | Health checks and metrics | All services |
| **Caching** | Response caching | High-frequency read operations |
| **Event Publishing** | Domain event handling | All domain services |
| **Configuration** | Environment-specific settings | All services |