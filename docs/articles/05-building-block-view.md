# 5. Building Block View

## 5.1 Level 1: System Overview

```mermaid
C4Container
    title BookWorm System - Level 1 Overview
    
    Person(customer, "Customer", "End user")
    Person(admin, "Administrator", "System admin")
    
    System_Boundary(bookworm, "BookWorm Platform") {
        Container(web, "Web Application", "React/Angular", "Customer-facing web interface")
        Container(gateway, "API Gateway", "YARP", "Request routing and authentication")
        Container(services, "Microservices", ".NET", "Business logic services")
        Container(messaging, "Message Bus", "RabbitMQ", "Event-driven communication")
        Container(data, "Data Layer", "PostgreSQL/Redis", "Data persistence and caching")
    }
    
    Rel(customer, web, "Uses", "HTTPS")
    Rel(admin, gateway, "Manages", "HTTPS")
    Rel(web, gateway, "API calls", "HTTPS")
    Rel(gateway, services, "Routes to", "HTTP/gRPC")
    Rel(services, messaging, "Publishes/Subscribes", "AMQP")
    Rel(services, data, "Reads/Writes", "SQL/Redis")
```

### Main Building Blocks

| Component | Responsibility | Interface |
|-----------|---------------|-----------|
| **Web Application** | User interface and experience | HTTP/HTTPS Browser |
| **API Gateway** | Request routing, authentication, rate limiting | REST API |
| **Microservices** | Business logic and domain operations | HTTP/gRPC |
| **Message Bus** | Asynchronous event communication | AMQP |
| **Data Layer** | Data persistence and caching | SQL/NoSQL |

## 5.2 Level 2: Service Decomposition

```mermaid
C4Container
    title BookWorm Services - Level 2 Decomposition
    
    System_Boundary(core, "Core Services") {
        Container(catalog, "Catalog Service", ".NET", "Book catalog management")
        Container(ordering, "Ordering Service", ".NET", "Order processing")
        Container(basket, "Basket Service", ".NET", "Shopping cart management")
        Container(rating, "Rating Service", ".NET", "Book ratings and reviews")
    }
    
    System_Boundary(support, "Supporting Services") {
        Container(chat, "Chat Service", ".NET", "AI-powered chat functionality")
        Container(notification, "Notification Service", ".NET", "User notifications")
        Container(finance, "Finance Service", ".NET", "Payment processing")
    }
    
    System_Boundary(infrastructure, "Infrastructure") {
        Container(gateway, "API Gateway", "YARP", "Request routing")
        Container(identity, "Identity Service", "Keycloak", "Authentication")
        Container(monitoring, "Health Checks UI", ".NET", "System monitoring")
    }
    
    Rel(ordering, catalog, "Product lookup", "gRPC")
    Rel(basket, catalog, "Product validation", "gRPC")
    Rel(ordering, finance, "Payment processing", "Events")
    Rel(ordering, notification, "Order updates", "Events")
```

### Service Responsibilities

#### Core Domain Services
| Service | Primary Responsibility | Key Operations |
|---------|----------------------|----------------|
| **Catalog Service** | Book catalog and inventory management | Search, browse, manage books |
| **Ordering Service** | Order lifecycle management | Create, process, fulfill orders |
| **Basket Service** | Shopping cart operations | Add/remove items, calculate totals |
| **Rating Service** | Reviews and ratings | Submit ratings, aggregate scores |

#### Supporting Services
| Service | Primary Responsibility | Key Operations |
|---------|----------------------|----------------|
| **Chat Service** | AI-powered customer support | Natural language processing, responses |
| **Notification Service** | User communication | Email, SMS, push notifications |
| **Finance Service** | Payment and financial operations | Process payments, handle refunds |

## 5.3 Level 3: Internal Service Structure

### Catalog Service Structure
```mermaid
graph TD
    A[Catalog API] --> B[Application Layer]
    B --> C[Domain Layer]
    B --> D[Infrastructure Layer]
    
    C --> E[Book Aggregate]
    C --> F[Author Aggregate]
    C --> G[Category Aggregate]
    C --> H[Publisher Aggregate]
    
    D --> I[Entity Framework]
    D --> J[Blob Storage]
    D --> K[Vector Database]
    D --> L[Event Bus]
```

### Vertical Slice Organization
Each service follows Vertical Slice Architecture with features organized as:

```
/Features
  /Books
    /Create
      - CreateBookCommand.cs
      - CreateBookHandler.cs
      - CreateBookValidator.cs
      - CreateBookEndpoint.cs
    /Update
      - UpdateBookCommand.cs
      - UpdateBookHandler.cs
      - UpdateBookValidator.cs
      - UpdateBookEndpoint.cs
```

## 5.4 Data Architecture

### Database Design Principles
- **Database per Service**: Each service owns its data
- **Shared Nothing Architecture**: No direct database access between services
- **Event-Driven Synchronization**: Data consistency through events
- **CQRS Implementation**: Separate read and write models

### Storage Patterns
| Pattern | Implementation | Usage |
|---------|---------------|-------|
| **Aggregate Storage** | Entity Framework Core | Transactional consistency |
| **Read Models** | Optimized projections | Query performance |
| **Event Store** | Custom implementation | Audit trail and replay |
| **Cache Layer** | Redis | Fast data access |
| **Blob Storage** | Azure Blob/S3 | File and image storage |

## 5.5 Cross-Cutting Concerns

### Shared Building Blocks
Located in `BookWorm.Chassis` and `BookWorm.SharedKernel`:

| Component | Purpose | Implementation |
|-----------|---------|---------------|
| **Activity Scope** | Request tracing | OpenTelemetry integration |
| **Command/Query Handlers** | CQRS implementation | MediatR patterns |
| **Event Bus** | Messaging abstraction | MassTransit wrapper |
| **Repository Pattern** | Data access abstraction | Generic repository |
| **Specification Pattern** | Query composition | LINQ expressions |
| **Validation** | Input validation | FluentValidation |
| **Mapping** | Object transformation | AutoMapper/Mapperly |
| **Versioning** | API versioning | ASP.NET Core versioning |