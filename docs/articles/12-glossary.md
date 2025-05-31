# 12. Glossary

## A

**Aggregate**  
A cluster of domain objects that can be treated as a single unit for data changes. Each aggregate has a root entity that controls access to the aggregate members.

**API Gateway**  
A service that acts as an entry point for all client requests, handling routing, authentication, rate limiting, and other cross-cutting concerns.

**Arc42**  
A template for architecture communication and documentation, providing a standardized structure for documenting software architectures.

**AsyncAPI**  
A specification for describing event-driven APIs, similar to OpenAPI but focused on asynchronous messaging patterns.

**Aspire (.NET Aspire)**  
Microsoft's cloud-native development framework that simplifies building distributed applications with built-in service discovery, configuration, and telemetry.

## B

**Bounded Context**  
A central pattern in Domain-Driven Design that defines the scope within which a particular domain model is defined and applicable.

**Bulkhead Pattern**  
An isolation pattern that partitions system resources to contain failures and prevent them from cascading to other parts of the system.

## C

**Circuit Breaker**  
A design pattern that prevents an application from repeatedly trying to execute an operation that's likely to fail, providing stability and preventing cascade failures.

**Command Query Responsibility Segregation (CQRS)**  
An architectural pattern that separates read and write operations into different models, optimizing each for their specific use cases.

**Compensating Action**  
An operation that undoes the work performed by a previously executed operation, used in distributed systems for handling failures.

**Container Orchestration**  
The automated management of containerized applications, including deployment, scaling, networking, and availability.

## D

**Domain-Driven Design (DDD)**  
An approach to software development that focuses on modeling software to match the business domain and business language.

**Domain Event**  
An event that represents something important that happened in the domain, used to trigger side effects or notify other bounded contexts.

**DocFX**  
A static documentation generator that can produce documentation from source code and Markdown files, commonly used for .NET projects.

## E

**Event Sourcing**  
A pattern where state changes are stored as a sequence of events, providing a complete audit trail and the ability to replay state.

**Event-Driven Architecture**  
An architectural pattern where components communicate primarily through events, promoting loose coupling and scalability.

**Eventual Consistency**  
A consistency model where the system will become consistent over time, allowing for temporary inconsistencies in distributed systems.

## F

**Feature Flag**  
A technique that allows features to be turned on or off without deploying new code, enabling gradual rollouts and A/B testing.

## G

**gRPC**  
A high-performance, language-agnostic remote procedure call framework that uses HTTP/2 and Protocol Buffers.

## H

**Health Check**  
A lightweight endpoint that indicates whether a service is running properly and ready to handle requests.

**Horizontal Scaling**  
The practice of adding more servers or instances to handle increased load, rather than upgrading existing hardware.

**HybridCache**  
A caching solution that combines multiple caching layers (memory, distributed) for optimal performance and reliability.

## I

**Inbox Pattern**  
A pattern that ensures idempotent message processing by tracking which messages have already been processed.

**Infrastructure as Code (IaC)**  
The practice of managing and provisioning computing infrastructure through machine-readable definition files.

**Istio**  
An open-source service mesh that provides traffic management, security, and observability for microservices.

## J

**JWT (JSON Web Token)**  
A compact, URL-safe means of representing claims to be transferred between two parties, commonly used for authentication.

## K

**Kubernetes**  
An open-source container orchestration platform for automating deployment, scaling, and management of containerized applications.

**Keycloak**  
An open-source identity and access management solution providing authentication and authorization services.

## L

**Load Balancer**  
A system that distributes incoming network traffic across multiple servers to ensure no single server bears too much demand.

**Loose Coupling**  
A design principle where components have minimal dependencies on each other, promoting flexibility and maintainability.

## M

**MediatR**  
A .NET library that implements the mediator pattern, promoting loose coupling by handling request/response and command/query patterns.

**Microservices**  
An architectural style that structures an application as a collection of small, independent services that communicate over well-defined APIs.

**Mutual TLS (mTLS)**  
A security protocol where both client and server authenticate each other using digital certificates.

## O

**OAuth 2.0**  
An authorization framework that enables third-party applications to obtain limited access to web services.

**OpenID Connect (OIDC)**  
An identity layer built on top of OAuth 2.0 that allows clients to verify user identity and obtain basic profile information.

**OpenTelemetry**  
A collection of tools, APIs, and SDKs for collecting, processing, and exporting telemetry data (metrics, logs, traces).

**Outbox Pattern**  
A pattern that ensures reliable message publishing by storing events in a database table within the same transaction as business data.

## P

**PostgreSQL**  
An open-source relational database management system emphasizing extensibility and SQL compliance.

**Protocol Buffers (protobuf)**  
A language-neutral, platform-neutral extensible mechanism for serializing structured data, used by gRPC.

## Q

**Qdrant**  
A vector database designed for storing and searching high-dimensional vectors, commonly used for AI and machine learning applications.

**Quality Attribute**  
A measurable or testable property of a system that indicates how well the system satisfies stakeholder needs.

## R

**RabbitMQ**  
An open-source message broker that implements the Advanced Message Queuing Protocol (AMQP).

**Redis**  
An in-memory data structure store used as a database, cache, and message broker.

**Resilience Pattern**  
Design patterns that help systems handle and recover from failures gracefully.

## S

**Saga Pattern**  
A design pattern for managing distributed transactions by breaking them into a series of local transactions with compensating actions.

**Service Discovery**  
The automatic detection of services and their network locations within a distributed system.

**Service Mesh**  
A dedicated infrastructure layer for handling service-to-service communication, providing features like load balancing, encryption, and observability.

**SonarQube**  
A platform for continuous inspection of code quality, providing static analysis to detect bugs, vulnerabilities, and code smells.

**Specification Pattern**  
A design pattern that encapsulates business rules in a reusable and combinable way, commonly used for querying and validation.

## T

**Telemetry**  
The automatic collection and transmission of data from remote sources, including metrics, logs, and traces.

**Testcontainers**  
A library that provides lightweight, throwaway instances of databases, message brokers, and other services for testing.

## V

**Vertical Slice Architecture**  
An architectural approach that organizes code by feature or use case rather than by technical layers.

**Value Object**  
A domain-driven design concept representing a descriptive aspect of the domain with no conceptual identity.

## Y

**YARP (Yet Another Reverse Proxy)**  
A reverse proxy toolkit for building fast proxy servers in .NET, commonly used for API gateways.

## Technical Acronyms

| Acronym | Full Form | Description |
|---------|-----------|-------------|
| **ACID** | Atomicity, Consistency, Isolation, Durability | Properties that guarantee database transactions are processed reliably |
| **AMQP** | Advanced Message Queuing Protocol | Application layer protocol for message-oriented middleware |
| **APM** | Application Performance Monitoring | Monitoring and management of software application performance |
| **CDN** | Content Delivery Network | Distributed network of servers for efficient content delivery |
| **CI/CD** | Continuous Integration/Continuous Deployment | Software development practices for automated testing and deployment |
| **CORS** | Cross-Origin Resource Sharing | Mechanism that allows web pages to access resources from other domains |
| **CRUD** | Create, Read, Update, Delete | Basic operations for persistent storage |
| **ELK** | Elasticsearch, Logstash, Kibana | Stack for searching, analyzing, and visualizing log data |
| **GDPR** | General Data Protection Regulation | European regulation on data protection and privacy |
| **HA** | High Availability | System design approach for ensuring operational continuity |
| **HTTP** | Hypertext Transfer Protocol | Foundation protocol for data communication on the web |
| **JSON** | JavaScript Object Notation | Lightweight data-interchange format |
| **ORM** | Object-Relational Mapping | Programming technique for converting data between incompatible type systems |
| **PCI** | Payment Card Industry | Security standards for handling credit card information |
| **REST** | Representational State Transfer | Architectural style for designing networked applications |
| **RPC** | Remote Procedure Call | Protocol for executing procedures on remote systems |
| **SLA** | Service Level Agreement | Commitment between service provider and client |
| **SQL** | Structured Query Language | Language for managing relational databases |
| **TLS** | Transport Layer Security | Cryptographic protocol for secure communication |
| **UI** | User Interface | Space where interactions between humans and machines occur |
| **UX** | User Experience | Overall experience of a person using a product or system |
| **WAF** | Web Application Firewall | Security system that monitors and filters HTTP traffic |

## Business Terms

**Book Catalog**  
The comprehensive collection of books available in the BookWorm system, including metadata, inventory, and availability information.

**Customer Journey**  
The complete experience a customer has with the BookWorm platform, from discovery to purchase and post-purchase support.

**Order Fulfillment**  
The complete process of receiving, processing, and delivering customer orders, including payment processing and shipping.

**Shopping Cart (Basket)**  
A virtual container that holds items selected by customers before completing their purchase.

**User Authentication**  
The process of verifying the identity of users accessing the BookWorm system.

**Inventory Management**  
The supervision of non-capitalized assets (books) and stock items, including availability tracking and restocking processes.

## Measurement Units

**Response Time**  
Measured in milliseconds (ms), representing the time taken to respond to a request.

**Throughput**  
Measured in requests per second (RPS) or transactions per minute (TPM), indicating system capacity.

**Availability**  
Expressed as a percentage (%), indicating the proportion of time a system is operational.

**Latency**  
Measured in milliseconds (ms), representing the delay in data transmission.

**Storage**  
Measured in bytes (B), kilobytes (KB), megabytes (MB), gigabytes (GB), or terabytes (TB).

**Memory**  
Typically measured in megabytes (MB) or gigabytes (GB) for application resource allocation.